using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using TKLua;
using Random = UnityEngine.Random;
using LuaFramework;

/// <summary>
/// 添加宏定义 MARBLE_DEV 可开启编辑功能
/// </summary>
[CSLStub.Out("Activity/Marble")]
public partial class MarbleLotteryMachine : CSLuaBehaviour
{
    [SerializeField] [Bind("")] protected TextAsset marbleLotteryPaths;
    [SerializeField] [Bind("ballTemplate")] protected MarbleLotteryMachineBall ballTemplate;
    [SerializeField] [Bind("obstacleTemplate")] protected MarbleLotteryMachineObstacle obstacleTemplate;
    [SerializeField] [Bind("finalAreaTemplate")] protected MarbleLotteryMachineFinalArea finalAreaTemplate;
    [SerializeField] [Bind("launcher")] protected MarbleLotteryMachineLauncher launcher;
    [SerializeField] [Bind("activityArea")] protected MarbleLotteryMachineActivityArea activityArea;
    [SerializeField] [Bind("obstacleContainer")] protected RectTransform obstacleContainer;
    [SerializeField] [Bind("finalAreaContainer")] protected RectTransform finalAreaContainer;
    [SerializeField] [Bind("ballContainer")] protected RectTransform ballContainer;

    public float gravity = 1000f;
    public float bounceFactor = 0.8f;
    public float initialSpeed = 400f;
    public float ballRadius = 15f;
    public float obstacleRadius = 25f;
    public float horizontalMargin = 0.15f;
    public float verticalMargin = 0.05f;
    public float blockHeight = 60f;
    public int blockCount = 8;
    public Color trueEndColor = new Color(0f, 1f, 0f, 1f);

    public float recycleDelay = 0f;
    public float launchInterval = 0.5f;
    public float rotateSpeed = 180f;

    [Header("路径优化")]
    public float pathSamplingDistance = 12f;
    public int bezierSegments = 8;

    public CSLList<GameObject> obstacles = new CSLList<GameObject>();
    public CSLList<CSLList<GameObject>> layerObstacles = new CSLList<CSLList<GameObject>>();
    public CSLList<GameObject> endBlocks = new CSLList<GameObject>();
    protected CSLList<float> targetXInObstacle = new CSLList<float>();
    protected CSLList<Button> targetSelectButtons = new CSLList<Button>();
    public int trueEndIndex = -1;
    public Vector2 startPoint = new Vector2(0f, 0f);
    public Vector2 endPoint = new Vector2(0f, 0f);
    public Rect activityRect = new Rect(0f, 0f, 0f, 0f);
    public float blockStartY = 0f;
    public float blockWidth = 0f;
    public GameObject gameContainer = null;
    public Canvas mainCanvas = null;

    public CSLList<Vector2> pathPoints = new CSLList<Vector2>();
    public CSLList<Vector2> bezierPoints = new CSLList<Vector2>();
    public CSLList<Vector2> finalPath = new CSLList<Vector2>();
    public LineRenderer pathLine = null;
    public CSLList<GameObject> collisionMarkers = new CSLList<GameObject>();

    [CSLStub.Out("Activity/Marble")]
    public class CollisionRecord
    {
        public int progress = 0;
        public int obstacleIndex = -1;
    }

    [CSLStub.Out("Activity/Marble")]
    public class BallInstance
    {
        public GameObject obj = null;
        public RectTransform rect = null;
#if MARBLE_DEV
        public Text label = null;
#endif
        public CSLList<Vector2> path = new CSLList<Vector2>();
        public float progress = 0f;
        public float totalLength = 0f;
        public float moveSpeed = 300f;
        public Vector2 velocity = new Vector2(0f, 0f);
        public bool isActive = false;
        public bool isComplete = false;
        public bool isRecycled = false;
        public float recycleProgress = 0f;
        public Vector3 recycleStartScale = new Vector3(1f, 1f, 1f);
        public bool isRecycling = false;
        public int ballID = 0;
        public int expectTargetIndex = -1;
        public int actualHitIndex = -1;
        public bool success = false;
        public MarbleLotteryMachineBall ballComponent = null;
        public float radius = 0f;
        public float completeTime = 0f;
        public CSLList<CollisionRecord> collisionRecords = new CSLList<CollisionRecord>();
        public int triggeredIndex = 0;
        public CSLList<int> triggeredRecordIndices = new CSLList<int>();
    }

    public CSLList<BallInstance> balls = new CSLList<BallInstance>();
    public int ballCounter = 0;
    private CSLList<GameObject> ballPool = new CSLList<GameObject>();

    public int pathGenAttempts = 0;
    protected Stopwatch pathGenStopwatch = new Stopwatch();
    public CSLList<Vector2> lastCollisionPoints = new CSLList<Vector2>();
    private CSLList<CollisionRecord> lastCollisionRecords = new CSLList<CollisionRecord>();

    [CSLStub.Out("Activity/Marble")]
    protected class ObstacleCollisionInfo
    {
        public Vector2 center = new Vector2(0f, 0f);
        public float radius = 0f;
        public GameObject obj = null;
        public MarbleLotteryMachineObstacle component = null;
    }
    
    [CSLStub.Out("Activity/Marble")]
    protected class FinalAreaCollisionInfo
    {
        public Rect rect = new Rect(0f, 0f, 0f, 0f);
        public GameObject obj = null;
        public MarbleLotteryMachineFinalArea component = null;
    }
    protected CSLList<ObstacleCollisionInfo> obstacleCollisionInfos = new CSLList<ObstacleCollisionInfo>();
    protected CSLList<FinalAreaCollisionInfo> finalAreaCollisionInfos = new CSLList<FinalAreaCollisionInfo>();

    [CSLStub.Out("Activity/Marble")]
    public class PathResult
    {
        public bool Success = false;
        public CSLList<Vector2> Path = new CSLList<Vector2>();
        public CSLList<CollisionRecord> Records = new CSLList<CollisionRecord>();
    }
    
    [CSLStub.Out("Activity/Marble")]
    public class SavedPathData
    {
        public CSLList<Vector2> finalPath = new CSLList<Vector2>();
        public int trueEndIndex = -1;
        public CSLList<CollisionRecord> collisionRecords = new CSLList<CollisionRecord>();
        public bool IsEmpty
        {
            get
            {
                bool isEmpty = (this.finalPath == null);
                if (!isEmpty)
                {
                    isEmpty = (this.finalPath.Count < 2);
                }
                return isEmpty;
            }
        }
    }

    public CSLList<CSLList<SavedPathData>> savedPaths = new CSLList<CSLList<SavedPathData>>();

    private enum RunState
    {
        Idle,
        RotatingToTarget,
        Launching,
        WaitingForNext,
        RotatingToIdle,
        WaitingForRecycle
    }

    private RunState currentRunState = RunState.Idle;
    private CSLList<int> runTargets = null;
    private int runTargetIndex = 0;
    private float stateTimer = 0f;
    private Quaternion startRotation = new Quaternion(0f, 0f, 0f, 1f);
    private Quaternion endRotation = new Quaternion(0f, 0f, 0f, 1f);
    private float rotationDuration = 0f;
    private Action runCompleteCallback = null;
    private bool isRunFinished = false;
    private CSLList<Vector2> currentPathForLaunch = null;
    private CSLList<CollisionRecord> currentCollisionRecordsForLaunch = null;
    private Quaternion launcherInitialRotation = new Quaternion(0f, 0f, 0f, 1f);

    public bool IsRunning { get; private set; } = false;

    public Button launchBtn = null;
    public Button resetBtn = null;
    public Button regenerateBtn = null;
    public Button generatePathBtn = null;
    public Text infoText = null;
    public Text runStatusText = null;
    public Button saveBtn = null;
    public Button savePersistentBtn = null;

    public static float Distance(Vector2 a, Vector2 b)
    {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        float x = a.x + (b.x - a.x) * t;
        float y = a.y + (b.y - a.y) * t;
        return new Vector2(x, y);
    }
    
    public static int ToInt(float value)
    {
        return (int)value;
    }

    public void SetInfoText(string text)
    {
        if (this.infoText != null)
        {
            this.infoText.text = text;
        }
    }

    public Canvas GetMainCanvas()
    {
        return this.mainCanvas;
    }

    public Transform GetGameContainer()
    {
        Transform result = null;
        if (this.gameContainer != null)
        {
            result = this.gameContainer.transform;
        }
        return result;
    }

    private void InitSavedPaths()
    {
        int count = Mathf.Max(1, this.blockCount);
        this.savedPaths.Clear();
        int i = 0;
        while (i < count)
        {
            this.savedPaths.Add(new CSLList<SavedPathData>());
            i = i + 1;
        }
    }

    void Awake()
    {
        if (this.marbleLotteryPaths == null)
        {
            GLog.Debug("MarbleLotteryMachine::Awake", "marbleLotteryPaths 未赋值");
        }

        InitSavedPaths();

#if MARBLE_DEV
        this.CreateUI();
#endif
        this.CreateSceneElements();
        this.pathGenStopwatch = new Stopwatch();
        this.lastCollisionPoints = new CSLList<Vector2>();
        this.LoadPathsFromBytes();
        bool launcherDestroyed = GameObjectUtils.IsDestroyed(this.launcher.gameObject);
        if (!launcherDestroyed)
        {
            this.launcherInitialRotation = this.launcher.transform.localRotation;
            GLog.Debug("MarbleLotteryMachine::ZZZ", this.launcherInitialRotation.ToString());
        }
        else
        {
            GLog.Debug("MarbleLotteryMachine::Awake", "launcher 未赋值或已销毁");
        }
    }

    void Start()
    {
        if (this.launchBtn != null)
        {
            this.launchBtn.onClick.AddListener(this.LaunchBall);
        }
        else
        {
            GLog.Debug("MarbleLotteryMachine::Start", "launchBtn 未赋值");
        }
        if (this.resetBtn != null)
        {
            this.resetBtn.onClick.AddListener(this.ResetAll);
        }
        if (this.regenerateBtn != null)
        {
            this.regenerateBtn.onClick.AddListener(this.RegenerateScene);
        }
        if (this.generatePathBtn != null)
        {
            this.generatePathBtn.onClick.AddListener(this.GeneratePath);
        }
        if (this.saveBtn != null)
        {
            this.saveBtn.onClick.AddListener(this.SaveCurrentPath);
        }
        if (this.savePersistentBtn != null)
        {
            this.savePersistentBtn.onClick.AddListener(this.OnSavePersistentClicked);
        }
        bool activityDestroyed = GameObjectUtils.IsDestroyed(this.activityArea.gameObject);
        if (activityDestroyed)
        {
            GLog.Debug("MarbleLotteryMachine::Start", "activityArea 未赋值或已销毁");
            return;
        }
        this.UpdateCollisionInfos();
#if MARBLE_DEV
        this.SelectSlot(0);
#endif
    }

    private void OnSavePersistentClicked()
    {
        this.SavePathsToBytes();
        this.SetInfoText("已保存所有路径到持久化目录");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            Run(new CSLList<int>()
            {
                1
            }, null);
        }
        this.UpdateBalls();
        this.RecycleBalls();
        this.UpdateRecycleAnimations();
        this.UpdateRunStateMachine();
        this.UpdateRunStatus();
    }

    public void ResetAll()
    {
        this.IsRunning = false;
        this.currentRunState = RunState.Idle;
        this.runTargets = null;
        this.runCompleteCallback = null;
        this.isRunFinished = false;

        int i = 0;
        while (i < this.balls.Count)
        {
            BallInstance ball = this.balls[i];
            if (ball.obj != null)
            {
                GameObject.Destroy(ball.obj);
            }
            i = i + 1;
        }
        this.balls.Clear();
        while (this.ballPool.Count > 0)
        {
            GameObject obj = this.ballPool[0];
            this.ballPool.RemoveAt(0);
            if (obj != null)
            {
                GameObject.Destroy(obj);
            }
        }
        this.ballCounter = 0;
        this.ClearPath();
        if (this.pathGenStopwatch != null)
        {
            bool isRunning = this.pathGenStopwatch.IsRunning;
            if (isRunning)
            {
                this.pathGenStopwatch.Stop();
            }
        }
        this.SetInfoText("已重置");
        bool launcherDestroyed = GameObjectUtils.IsDestroyed(this.launcher.gameObject);
        if (!launcherDestroyed)
        {
            this.launcher.transform.localRotation = this.launcherInitialRotation;
        }
    }

    public void RegenerateScene()
    {
        this.ResetAll();
        InitSavedPaths();

        bool obsDestroyed = GameObjectUtils.IsDestroyed(this.obstacleTemplate.gameObject);
        bool finalDestroyed = GameObjectUtils.IsDestroyed(this.finalAreaTemplate.gameObject);
        if (obsDestroyed || finalDestroyed)
        {
            GLog.Debug("MarbleLotteryMachine::RegenerateScene", "obstacleTemplate 或 finalAreaTemplate 未赋值或已销毁");
            return;
        }
        this.GenerateObstacles();
        this.GenerateEndBlocks();
        this.ClearPath();
        this.pathGenAttempts = 0;
        bool activityDestroyed = GameObjectUtils.IsDestroyed(this.activityArea.gameObject);
        if (activityDestroyed)
        {
            return;
        }
        this.UpdateCollisionInfos();
        this.SetInfoText("场景已重生，点击生成路径或发射");
    }

    protected float GetBallRadius(GameObject ballObj)
    {
        float result = 15f;
        if (ballObj == null)
        {
            return result;
        }
        MarbleLotteryMachineBall comp = LuaBehaviour.getLuaBehaviour<MarbleLotteryMachineBall>(ballObj);
        bool compDestroyed = GameObjectUtils.IsDestroyed(comp.gameObject);
        bool areaDestroyed = GameObjectUtils.IsDestroyed(comp.CollisionArea);
        if (!compDestroyed && !areaDestroyed)
        {
            result = comp.CollisionArea.rect.width * 0.5f;
        }
        return result;
    }

    private bool IsValidVector2(Vector2 v)
    {
        bool validX = !float.IsNaN(v.x);
        validX = validX && !float.IsInfinity(v.x);
        bool validY = !float.IsNaN(v.y);
        validY = validY && !float.IsInfinity(v.y);
        return (validX && validY);
    }

    private void RemapCollisionRecords(CSLList<CollisionRecord> records)
    {
        if (records == null)
        {
            return;
        }
        int i = 0;
        while (i < records.Count)
        {
            CollisionRecord rec = records[i];
            bool indexValid = (rec.obstacleIndex >= 0);
            if (indexValid)
            {
                indexValid = (rec.obstacleIndex < this.obstacles.Count);
            }
            if (!indexValid)
            {
                rec.obstacleIndex = -1;
            }
            i = i + 1;
        }
    }

    private GameObject GetOrCreateBallObject()
    {
        GameObject result = null;
        bool templateDestroyed = GameObjectUtils.IsDestroyed(this.ballTemplate.gameObject);
        if (templateDestroyed)
        {
            GLog.Debug("MarbleLotteryMachine::GetOrCreateBallObject", "ballTemplate 未赋值或已销毁");
            return result;
        }
        if (this.ballPool.Count > 0)
        {
            GameObject obj = this.ballPool[0];
            this.ballPool.RemoveAt(0);
            if (obj != null)
            {
                obj.SetActive(true);
                obj.transform.localScale = new Vector3(1f, 1f, 1f);
                result = obj;
                return result;
            }
        }
        result = GameObject.Instantiate(this.ballTemplate.gameObject);
        return result;
    }

    private void RecycleBalls()
    {
        int i = this.balls.Count - 1;
        while (i >= 0)
        {
            BallInstance ball = this.balls[i];
            bool canRecycle = ball.isComplete;
            if (canRecycle)
            {
                canRecycle = !ball.isRecycled;
            }
            if (canRecycle)
            {
                canRecycle = !ball.isRecycling;
            }
            if (canRecycle)
            {
                float elapsed = Time.time - ball.completeTime;
                if (elapsed >= this.recycleDelay)
                {
                    ball.isRecycling = true;
                    ball.recycleProgress = 0f;
                    ball.recycleStartScale = ball.obj.transform.localScale;
                }
            }
            i = i - 1;
        }
    }

    private void UpdateRecycleAnimations()
    {
        int i = this.balls.Count - 1;
        while (i >= 0)
        {
            BallInstance ball = this.balls[i];
            if (!ball.isRecycling)
            {
                i = i - 1;
                continue;
            }
            ball.recycleProgress = 1f;
            ball.obj.transform.localScale = new Vector3(0f, 0f, 0f);
            ball.obj.SetActive(false);
            this.ballPool.Add(ball.obj);
            ball.obj = null;
            ball.rect = null;
            ball.ballComponent = null;
            this.balls.RemoveAt(i);
            i = i - 1;
        }
    }

    public void Run(CSLList<int> targets, Action onComplete)
    {
        if (this.IsRunning)
        {
            GLog.Debug("MarbleLotteryMachine::Run", "Machine is already running.");
            return;
        }
        if (this.balls.Count > 0)
        {
            GLog.Debug("MarbleLotteryMachine::Run", "还有 " + this.balls.Count + " 个小球未回收，请等待。");
            this.SetInfoText("还有 " + this.balls.Count + " 个小球未回收，请等待...");
            return;
        }

        if (targets == null)
        {
            if (onComplete != null)
            {
                onComplete();
            }
            return;
        }
        if (targets.Count == 0)
        {
            if (onComplete != null)
            {
                onComplete();
            }
            return;
        }

        this.IsRunning = true;
        this.currentRunState = RunState.RotatingToTarget;
        this.runTargets = new CSLList<int>(targets);
        this.runTargetIndex = 0;
        this.runCompleteCallback = onComplete;
        this.isRunFinished = false;
        this.stateTimer = 0f;

        this.SetupNextTarget();
        GLog.Debug("MarbleLotteryMachine::Run", "Run 开始，共 " + targets.Count + " 个目标");
    }

    private void UpdateRunStateMachine()
    {
        if (!this.IsRunning)
        {
            return;
        }

        RunState state = this.currentRunState;
        if (state == RunState.RotatingToTarget)
        {
            this.stateTimer = this.stateTimer + Time.deltaTime;
            float t = Mathf.Clamp01(this.stateTimer / this.rotationDuration);
            bool launcherDestroyed = GameObjectUtils.IsDestroyed(this.launcher.gameObject);
            if (!launcherDestroyed)
            {
                this.launcher.transform.localRotation = Quaternion.Slerp(this.startRotation, this.endRotation, t);
                if (t >= 1f)
                {
                    this.launcher.transform.localRotation = this.endRotation;
                    this.currentRunState = RunState.Launching;
                }
            }
            else
            {
                GLog.Debug("MarbleLotteryMachine::UpdateRunStateMachine", "launcher 未赋值或已销毁");
                this.currentRunState = RunState.Launching;
            }
        }
        else if (state == RunState.Launching)
        {
            this.LaunchCurrentBall();
            if (this.runTargetIndex < this.runTargets.Count)
            {
                this.currentRunState = RunState.WaitingForNext;
                this.stateTimer = 0f;
            }
            else
            {
                this.currentRunState = RunState.RotatingToIdle;
                this.stateTimer = 0f;
                this.SetupRotateToIdle();
            }
        }
        else if (state == RunState.WaitingForNext)
        {
            this.stateTimer = this.stateTimer + Time.deltaTime;
            if (this.stateTimer >= this.launchInterval)
            {
                this.SetupNextTarget();
                this.currentRunState = RunState.RotatingToTarget;
            }
        }
        else if (state == RunState.RotatingToIdle)
        {
            this.stateTimer = this.stateTimer + Time.deltaTime;
            float t2 = Mathf.Clamp01(this.stateTimer / this.rotationDuration);
            bool launcherDestroyed = GameObjectUtils.IsDestroyed(this.launcher.gameObject);
            if (!launcherDestroyed)
            {
                this.launcher.transform.localRotation = Quaternion.Slerp(this.startRotation, this.endRotation, t2);
                if (t2 >= 1f)
                {
                    this.launcher.transform.localRotation = this.endRotation;
                    this.isRunFinished = true;
                    this.currentRunState = RunState.WaitingForRecycle;
                }
            }
            else
            {
                this.isRunFinished = true;
                this.currentRunState = RunState.WaitingForRecycle;
            }
        }
        else if (state == RunState.WaitingForRecycle)
        {
            if (this.balls.Count == 0)
            {
                this.IsRunning = false;
                this.currentRunState = RunState.Idle;
                if (this.runCompleteCallback != null)
                {
                    this.runCompleteCallback();
                }
                this.runCompleteCallback = null;
                this.runTargets = null;
                GLog.Debug("MarbleLotteryMachine::UpdateRunStateMachine", "Run 完全结束（所有小球已回收）");
            }
        }
    }

    private void SetupNextTarget()
    {
        if (this.runTargetIndex >= this.runTargets.Count)
        {
            return;
        }
        this.UpdateCollisionInfos();

        int target = this.runTargets[this.runTargetIndex];
        CSLList<Vector2> path = null;
        CSLList<CollisionRecord> collisionRecs = null;

        bool targetValid = (target >= 0);
        if (targetValid)
        {
            targetValid = (target < this.savedPaths.Count);
        }
        if (targetValid)
        {
            CSLList<SavedPathData> slot = this.savedPaths[target];
            if (slot != null)
            {
                if (slot.Count > 0)
                {
                    int idx = Random.Range(0, slot.Count);
                    SavedPathData data = slot[idx];
                    path = new CSLList<Vector2>(data.finalPath);
                    collisionRecs = new CSLList<CollisionRecord>();
                    int r = 0;
                    while (r < data.collisionRecords.Count)
                    {
                        CollisionRecord src = data.collisionRecords[r];
                        collisionRecs.Add(new CollisionRecord { progress = src.progress, obstacleIndex = src.obstacleIndex });
                        r = r + 1;
                    }
                    if (path != null)
                    {
                        if (path.Count > 0)
                        {
                            Vector2 offset = this.startPoint - path[0];
                            int j = 0;
                            while (j < path.Count)
                            {
                                path[j] = new Vector2(path[j].x + offset.x, path[j].y + offset.y);
                                j = j + 1;
                            }
                        }
                    }
                    this.RemapCollisionRecords(collisionRecs);
                }
            }
        }

        if (path == null)
        {
            PathResult result = this.GeneratePathForTarget(target);
            if (result != null)
            {
                if (result.Success)
                {
                    path = result.Path;
                    collisionRecs = result.Records;
                    this.RemapCollisionRecords(collisionRecs);
                }
            }
        }
        if (path == null)
        {
            GLog.Debug("MarbleLotteryMachine::SetupNextTarget", "无法获取目标 " + target + " 的路径，跳过");
            this.runTargetIndex = this.runTargetIndex + 1;
            if (this.runTargetIndex < this.runTargets.Count)
            {
                this.SetupNextTarget();
            }
            else
            {
                this.isRunFinished = true;
                this.currentRunState = RunState.WaitingForRecycle;
            }
            return;
        }
        if (path.Count < 2)
        {
            GLog.Debug("MarbleLotteryMachine::SetupNextTarget", "路径点数不足，跳过目标 " + target);
            this.runTargetIndex = this.runTargetIndex + 1;
            if (this.runTargetIndex < this.runTargets.Count)
            {
                this.SetupNextTarget();
            }
            else
            {
                this.isRunFinished = true;
                this.currentRunState = RunState.WaitingForRecycle;
            }
            return;
        }

        int k = 0;
        bool hasInvalid = false;
        while (k < path.Count)
        {
            Vector2 p = path[k];
            if (!this.IsValidVector2(p))
            {
                hasInvalid = true;
                break;
            }
            k = k + 1;
        }
        if (hasInvalid)
        {
            GLog.Debug("MarbleLotteryMachine::SetupNextTarget", "路径包含无效点，跳过目标 " + target);
            this.runTargetIndex = this.runTargetIndex + 1;
            if (this.runTargetIndex < this.runTargets.Count)
            {
                this.SetupNextTarget();
            }
            else
            {
                this.isRunFinished = true;
                this.currentRunState = RunState.WaitingForRecycle;
            }
            return;
        }

        this.currentPathForLaunch = path;
        this.currentCollisionRecordsForLaunch = collisionRecs;

        bool launcherDestroyed = GameObjectUtils.IsDestroyed(this.launcher.gameObject);
        if (launcherDestroyed)
        {
            GLog.Debug("MarbleLotteryMachine::SetupNextTarget", "launcher 未赋值或已销毁");
            return;
        }

        Vector2 dir = (path[1] - path[0]).normalized;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

        this.startRotation = this.launcher.transform.localRotation;
        this.endRotation = targetRotation;
        float deltaAngle = Mathf.DeltaAngle(this.startRotation.eulerAngles.z, this.endRotation.eulerAngles.z);
        this.rotationDuration = Mathf.Abs(deltaAngle) / this.rotateSpeed;
        if (this.rotationDuration < 0.01f)
        {
            this.rotationDuration = 0.01f;
        }
        this.stateTimer = 0f;
    }

    private void SetupRotateToIdle()
    {
        bool launcherDestroyed = GameObjectUtils.IsDestroyed(this.launcher.gameObject);
        if (launcherDestroyed)
        {
            GLog.Debug("MarbleLotteryMachine::SetupRotateToIdle", "launcher 未赋值或已销毁");
            return;
        }
        this.startRotation = this.launcher.transform.localRotation;
        this.endRotation = this.launcherInitialRotation;
        float deltaAngle = Mathf.DeltaAngle(this.startRotation.eulerAngles.z, this.endRotation.eulerAngles.z);
        this.rotationDuration = Mathf.Abs(deltaAngle) / this.rotateSpeed;
        if (this.rotationDuration < 0.01f)
        {
            this.rotationDuration = 0.01f;
        }
        this.stateTimer = 0f;
    }

    private void LaunchCurrentBall()
    {
        if (this.currentPathForLaunch == null)
        {
            GLog.Debug("MarbleLotteryMachine::LaunchCurrentBall", "发射时路径无效");
            this.runTargetIndex = this.runTargetIndex + 1;
            return;
        }
        if (this.currentPathForLaunch.Count < 2)
        {
            GLog.Debug("MarbleLotteryMachine::LaunchCurrentBall", "发射时路径无效");
            this.runTargetIndex = this.runTargetIndex + 1;
            return;
        }
        int target = this.runTargets[this.runTargetIndex];
        this.LaunchBallWithPath(this.currentPathForLaunch, target, this.currentCollisionRecordsForLaunch);
        this.runTargetIndex = this.runTargetIndex + 1;
    }

    private void LaunchBallWithPath(CSLList<Vector2> path, int targetIndex, CSLList<CollisionRecord> collisionRecords)
    {
        if (path == null)
        {
            GLog.Debug("MarbleLotteryMachine::LaunchBallWithPath", "路径无效，无法发射");
            return;
        }
        if (path.Count < 2)
        {
            GLog.Debug("MarbleLotteryMachine::LaunchBallWithPath", "路径无效，无法发射");
            return;
        }
        bool launcherDestroyed = GameObjectUtils.IsDestroyed(this.launcher.gameObject);
        if (launcherDestroyed)
        {
            GLog.Debug("MarbleLotteryMachine::LaunchBallWithPath", "launcher 未赋值或已销毁");
            return;
        }
        bool launchPosDestroyed = GameObjectUtils.IsDestroyed(this.launcher.LaunchPos);
        if (launchPosDestroyed)
        {
            GLog.Debug("MarbleLotteryMachine::LaunchBallWithPath", "launcher 未赋值或已销毁");
            return;
        }
        bool templateDestroyed = GameObjectUtils.IsDestroyed(this.ballTemplate.gameObject);
        if (templateDestroyed)
        {
            GLog.Debug("MarbleLotteryMachine::LaunchBallWithPath", "ballTemplate 未赋值或已销毁");
            return;
        }
        bool containerDestroyed = GameObjectUtils.IsDestroyed(this.ballContainer);
        if (containerDestroyed)
        {
            GLog.Debug("MarbleLotteryMachine::LaunchBallWithPath", "ballContainer 未赋值或已销毁");
            return;
        }

        Vector2 worldPos = this.launcher.LaunchPos.position;
        if (!this.IsValidVector2(worldPos))
        {
            GLog.Debug("MarbleLotteryMachine::LaunchBallWithPath", "发射位置无效 " + worldPos.ToString());
            return;
        }
        Vector2 containerLocalPos = this.ballContainer.InverseTransformPoint(worldPos);
        if (!this.IsValidVector2(containerLocalPos))
        {
            GLog.Debug("MarbleLotteryMachine::LaunchBallWithPath", "转换后的局部位置无效 " + containerLocalPos.ToString());
            return;
        }

        GameObject ballObj = this.GetOrCreateBallObject();
        if (ballObj == null)
        {
            GLog.Debug("MarbleLotteryMachine::LaunchBallWithPath", "获取小球对象失败");
            return;
        }
        ballObj.transform.SetParent(this.ballContainer, false);
        ballObj.SetActive(true);
        RectTransform rect = ballObj.gameObject.GetComponent<RectTransform>();
        rect.localPosition = new Vector3(containerLocalPos.x, containerLocalPos.y, 0f);

        MarbleLotteryMachineBall ballComp = LuaBehaviour.getLuaBehaviour<MarbleLotteryMachineBall>(ballObj);
        float radius = this.GetBallRadius(ballObj);

        BallInstance newBall = new BallInstance();
        this.ballCounter = this.ballCounter + 1;
        newBall.ballID = this.ballCounter;
        newBall.expectTargetIndex = targetIndex;
        newBall.path = path;
        newBall.progress = 0f;
        newBall.totalLength = 0f;
        int i = 0;
        while (i < path.Count - 1)
        {
            float segLen = MarbleLotteryMachine.Distance(path[i], path[i + 1]);
            newBall.totalLength = newBall.totalLength + segLen;
            i = i + 1;
        }
        newBall.moveSpeed = this.initialSpeed * 1.5f;
        newBall.isActive = true;
        newBall.obj = ballObj;
        newBall.rect = rect;
        newBall.ballComponent = ballComp;
        newBall.radius = radius;
        newBall.collisionRecords.Clear();
        if (collisionRecords != null)
        {
            int r = 0;
            while (r < collisionRecords.Count)
            {
                CollisionRecord rec = collisionRecords[r];
                newBall.collisionRecords.Add(new CollisionRecord { progress = rec.progress, obstacleIndex = rec.obstacleIndex });
                r = r + 1;
            }
        }
        newBall.triggeredIndex = 0;
        newBall.triggeredRecordIndices = new CSLList<int>();
#if MARBLE_DEV
        newBall.label = this.CreateLabel(ballObj, newBall.ballID + "→" + targetIndex);
        if (newBall.label != null)
        {
            newBall.label.color = new Color(0f, 0f, 0f, 1f);
        }
#endif
        this.balls.Add(newBall);

        this.SetInfoText("发射路径小球 #" + newBall.ballID + " → 目标 " + targetIndex + "，当前小球总数 " + this.balls.Count);
        GLog.Debug("MarbleLotteryMachine::LaunchBallWithPath", "小球 #" + newBall.ballID + "，目标 " + targetIndex + "，路径点 " + path.Count + "，碰撞记录 " + newBall.collisionRecords.Count);
    }

    public PathResult GeneratePathForTarget(int targetIndex)
    {
        if (this.finalAreaCollisionInfos == null)
        {
            return null;
        }
        bool indexValid = (targetIndex >= 0);
        if (indexValid)
        {
            indexValid = (targetIndex < this.finalAreaCollisionInfos.Count);
        }
        if (!indexValid)
        {
            return null;
        }
        int oldTrueEnd = this.trueEndIndex;
        PathResult result = this.GenerateValidPath(targetIndex, this.ballRadius);
        this.trueEndIndex = oldTrueEnd;
        return result;
    }

    private void UpdateBalls()
    {
        bool activityDestroyed = GameObjectUtils.IsDestroyed(this.activityArea.gameObject);
        if (activityDestroyed)
        {
            return;
        }

        int b = 0;
        while (b < this.balls.Count)
        {
            BallInstance ball = this.balls[b];
            bool active = ball.isActive;
            bool complete = ball.isComplete;
            bool shouldProcess = (active && !complete);

            if (shouldProcess)
            {
                // ---- 路径跟随模式（优先） ----
                bool hasPath = (ball.path != null);
                if (hasPath)
                {
                    hasPath = (ball.path.Count >= 2);
                }

                if (hasPath)
                {
                    if (ball.totalLength > 0.001f)
                    {
                        float speed = ball.moveSpeed;
                        if (speed <= 0f)
                        {
                            speed = this.initialSpeed;
                        }

                        float step = speed * Time.deltaTime / ball.totalLength;
                        ball.progress = ball.progress + step;
                        if (ball.progress > 1f)
                        {
                            ball.progress = 1f;
                        }

                        Vector2 pathLocalPos = this.GetPointOnPath(ball.path, ball.progress);
                        Vector2 pathWorldPos = this.activityArea.ActivityArea.TransformPoint(pathLocalPos);
                        ball.rect.position = pathWorldPos;
                        ball.rect.localPosition = new Vector3(ball.rect.localPosition.x, ball.rect.localPosition.y, 0f);

                        // 基于万分比进度触发碰撞
                        bool hasComponent = (ball.ballComponent != null);
                        if (hasComponent)
                        {
                            bool hasRecords = (ball.collisionRecords != null);
                            if (hasRecords)
                            {
                                hasRecords = (ball.collisionRecords.Count > 0);
                            }

                            if (hasRecords)
                            {
                                // 先标记墙壁记录
                                int i = 0;
                                while (i < ball.collisionRecords.Count)
                                {
                                    bool already = ball.triggeredRecordIndices.Contains(i);
                                    if (!already)
                                    {
                                        if (ball.collisionRecords[i].obstacleIndex == -1)
                                        {
                                            ball.triggeredRecordIndices.Add(i);
                                        }
                                    }

                                    i = i + 1;
                                }

                                // 检查未触发的有效记录
                                i = 0;
                                while (i < ball.collisionRecords.Count)
                                {
                                    bool already = ball.triggeredRecordIndices.Contains(i);
                                    if (!already)
                                    {
                                        CollisionRecord rec = ball.collisionRecords[i];
                                        if (rec.obstacleIndex != -1)
                                        {
                                            int currentProgressTenThousandth = ToInt(ball.progress * 10000f);
                                            if (currentProgressTenThousandth >= rec.progress)
                                            {
                                                bool obsIndexValid = (rec.obstacleIndex >= 0);
                                                if (obsIndexValid)
                                                {
                                                    obsIndexValid = (rec.obstacleIndex < this.obstacles.Count);
                                                }

                                                if (obsIndexValid)
                                                {
                                                    GameObject obsObj = this.obstacles[rec.obstacleIndex];
                                                    if (obsObj != null)
                                                    {
                                                        MarbleLotteryMachineObstacle obsComp =
                                                            LuaBehaviour.getLuaBehaviour<MarbleLotteryMachineObstacle>(
                                                                obsObj);
                                                        bool compDestroyed =
                                                            GameObjectUtils.IsDestroyed(obsComp.gameObject);
                                                        if (!compDestroyed)
                                                        {
                                                            obsComp.Hit();
                                                            bool ballCompDestroyed =
                                                                GameObjectUtils.IsDestroyed(ball.ballComponent
                                                                    .gameObject);
                                                            if (!ballCompDestroyed)
                                                            {
                                                                ball.ballComponent.HitObstacle();
                                                            }

                                                            GLog.Debug("MarbleLotteryMachine::UpdateBalls",
                                                                "小球 #" + ball.ballID + " 触发万分比进度 " + rec.progress +
                                                                " 障碍物 " + rec.obstacleIndex);
                                                        }
                                                    }
                                                }

                                                ball.triggeredRecordIndices.Add(i);
                                            }
                                        }
                                    }

                                    i = i + 1;
                                }
                            }
                        }

                        // 到达终点
                        if (ball.progress >= 1f)
                        {
                            ball.isActive = false;
                            ball.isComplete = true;
                            ball.completeTime = Time.time;
                            Vector2 finalPos = ball.path[ball.path.Count - 1];
                            ball.rect.position = this.activityArea.ActivityArea.TransformPoint(finalPos);
                            ball.actualHitIndex = this.CheckHitEnd(finalPos);
                            bool success = (ball.actualHitIndex == ball.expectTargetIndex);
                            ball.success = success;
                            if (success)
                            {
                                bool idxValid = (ball.actualHitIndex >= 0);
                                if (idxValid)
                                {
                                    idxValid = (ball.actualHitIndex < this.finalAreaCollisionInfos.Count);
                                }

                                if (idxValid)
                                {
                                    FinalAreaCollisionInfo areaInfo = this.finalAreaCollisionInfos[ball.actualHitIndex];
                                    bool compDestroyed = GameObjectUtils.IsDestroyed(areaInfo.component.gameObject);
                                    if (!compDestroyed)
                                    {
                                        areaInfo.component.Reach();
                                    }

                                    bool ballCompDestroyed = GameObjectUtils.IsDestroyed(ball.ballComponent.gameObject);
                                    if (!ballCompDestroyed)
                                    {
                                        ball.ballComponent.ReachFinalArea();
                                    }
                                }
                            }

                            this.LogResult(ball);
                            this.UpdateTargetLabels();
                        }
                    }
                    // 路径分支结束，不执行物理模拟
                }
                else
                {
                    // ---- 物理模拟模式 ----
                    float dt = Time.deltaTime;
                    if (dt > 0.02f)
                    {
                        dt = 0.02f;
                    }

                    Vector3 worldPos = ball.rect.position;
                    Vector2 localPos = this.activityArea.ActivityArea.InverseTransformPoint(worldPos);
                    ball.velocity.y = ball.velocity.y - this.gravity * dt;
                    Vector2 newLocalPos = new Vector2(localPos.x + ball.velocity.x * dt,
                        localPos.y + ball.velocity.y * dt);

                    int obsIdx = 0;
                    while (obsIdx < this.obstacleCollisionInfos.Count)
                    {
                        ObstacleCollisionInfo info = this.obstacleCollisionInfos[obsIdx];
                        Vector2 toBall = newLocalPos - info.center;
                        float dist = toBall.magnitude;
                        if (dist < ball.radius + info.radius)
                        {
                            if (dist > 0.001f)
                            {
                                Vector2 normal = toBall.normalized;
                                newLocalPos = info.center + normal * (ball.radius + info.radius);
                                float velAlongNormal = Vector2.Dot(ball.velocity, normal);
                                if (velAlongNormal < 0f)
                                {
                                    ball.velocity = ball.velocity - normal * 2f * velAlongNormal * this.bounceFactor;
                                }

                                if (info.component != null)
                                {
                                    info.component.Hit();
                                }

                                if (ball.ballComponent != null)
                                {
                                    ball.ballComponent.HitObstacle();
                                }

                                break;
                            }
                        }

                        obsIdx = obsIdx + 1;
                    }

                    // 边界碰撞
                    if (newLocalPos.x - ball.radius < this.activityRect.xMin)
                    {
                        newLocalPos.x = this.activityRect.xMin + ball.radius;
                        if (ball.velocity.x < 0f)
                        {
                            ball.velocity.x = ball.velocity.x * -this.bounceFactor;
                        }
                    }

                    if (newLocalPos.x + ball.radius > this.activityRect.xMax)
                    {
                        newLocalPos.x = this.activityRect.xMax - ball.radius;
                        if (ball.velocity.x > 0f)
                        {
                            ball.velocity.x = ball.velocity.x * -this.bounceFactor;
                        }
                    }

                    if (newLocalPos.y + ball.radius > this.activityRect.yMax)
                    {
                        newLocalPos.y = this.activityRect.yMax - ball.radius;
                        if (ball.velocity.y > 0f)
                        {
                            ball.velocity.y = -Mathf.Abs(ball.velocity.y) * 0.2f;
                        }
                        else
                        {
                            ball.velocity.y = -1f;
                        }
                    }

                    if (newLocalPos.y - ball.radius < this.activityRect.yMin)
                    {
                        newLocalPos.y = this.activityRect.yMin + ball.radius;
                        if (ball.velocity.y < 0f)
                        {
                            ball.velocity.y = ball.velocity.y * -this.bounceFactor;
                        }
                    }

                    ball.rect.position = this.activityArea.ActivityArea.TransformPoint(newLocalPos);
                    int hitIdx = this.CheckHitEnd(newLocalPos);
                    bool reached = (hitIdx >= 0);
                    if (reached)
                    {
                        ball.actualHitIndex = hitIdx;
                        ball.success = (hitIdx == ball.expectTargetIndex);
                        bool idxValid = (hitIdx < this.finalAreaCollisionInfos.Count);
                        if (idxValid)
                        {
                            FinalAreaCollisionInfo areaInfo = this.finalAreaCollisionInfos[hitIdx];
                            bool compDestroyed = GameObjectUtils.IsDestroyed(areaInfo.component.gameObject);
                            if (!compDestroyed)
                            {
                                areaInfo.component.Reach();
                            }

                            bool ballCompDestroyed = GameObjectUtils.IsDestroyed(ball.ballComponent.gameObject);
                            if (!ballCompDestroyed)
                            {
                                ball.ballComponent.ReachFinalArea();
                            }
                        }
                    }

                    if (reached)
                    {
                        ball.isActive = false;
                        ball.isComplete = true;
                        ball.completeTime = Time.time;
                        this.LogResult(ball);
                        this.UpdateTargetLabels();
                    }
                    else
                    {
                        if (newLocalPos.y - ball.radius < this.activityRect.yMin + 5f)
                        {
                            ball.isActive = false;
                            ball.isComplete = true;
                            ball.completeTime = Time.time;
                            ball.actualHitIndex = this.CheckHitEnd(newLocalPos);
                            ball.success = (ball.actualHitIndex == ball.expectTargetIndex);
                            this.LogResult(ball);
                            this.UpdateTargetLabels();
                        }
                    }
                }
            }

            // 统一步进
            b = b + 1;
        }
    }

    private Vector2 GetPointOnPath(CSLList<Vector2> path, float t)
    {
        if (path == null)
        {
            return new Vector2(0f, 0f);
        }
        if (path.Count == 0)
        {
            return new Vector2(0f, 0f);
        }
        if (t <= 0f)
        {
            return path[0];
        }
        if (t >= 1f)
        {
            return path[path.Count - 1];
        }

        float totalLen = 0f;
        int i = 0;
        while (i < path.Count - 1)
        {
            float segLen = MarbleLotteryMachine.Distance(path[i], path[i + 1]);
            bool invalid = float.IsNaN(segLen);
            if (!invalid)
            {
                invalid = float.IsInfinity(segLen);
            }
            if (invalid)
            {
                GLog.Debug("MarbleLotteryMachine::GetPointOnPath", "路径段长度无效，i=" + i + ", p0=" + path[i].ToString() + ", p1=" + path[i + 1].ToString());
                return path[0];
            }
            totalLen = totalLen + segLen;
            i = i + 1;
        }
        if (totalLen <= 0.0001f)
        {
            return path[0];
        }

        float targetLen = t * totalLen;
        float accumulated = 0f;
        i = 0;
        while (i < path.Count - 1)
        {
            float segLen = MarbleLotteryMachine.Distance(path[i], path[i + 1]);
            if (accumulated + segLen >= targetLen)
            {
                float localT = (targetLen - accumulated) / segLen;
                if (float.IsNaN(localT))
                {
                    localT = 0f;
                }
                float clamped = Mathf.Clamp01(localT);
                return MarbleLotteryMachine.Lerp(path[i], path[i + 1], clamped);
            }
            accumulated = accumulated + segLen;
            i = i + 1;
        }
        return path[path.Count - 1];
    }

    private void LogResult(BallInstance ball)
    {
        string log = "小球 #" + ball.ballID + " 预期 " + ball.expectTargetIndex + " 实际 " + ball.actualHitIndex + " 成功: " + ball.success;
        GLog.Debug("MarbleLotteryMachine::LogResult", log);
        this.SetInfoText(log);
    }

    private void UpdateTargetLabels()
    {
        int i = 0;
        while (i < this.endBlocks.Count)
        {
            GameObject block = this.endBlocks[i];
            if (block != null)
            {
                Transform old = block.transform.Find("TargetLabel");
                if (old != null)
                {
                    GameObject.Destroy(old.gameObject);
                }
            }
            i = i + 1;
        }
        CSLDictionary<int, CSLList<int>> targetMap = new CSLDictionary<int, CSLList<int>>();
        int j = 0;
        while (j < this.balls.Count)
        {
            BallInstance ball = this.balls[j];
            if (!ball.isComplete)
            {
                int idx = ball.expectTargetIndex;
                bool hasKey = targetMap.ContainsKey(idx);
                if (!hasKey)
                {
                    targetMap[idx] = new CSLList<int>();
                }
                targetMap[idx].Add(ball.ballID);
            }
            j = j + 1;
        }
        var keys = targetMap.Keys;
        foreach (int k in keys)
        {
            int blockIdx = k;
            if (blockIdx < 0)
            {
                continue;
            }
            if (blockIdx >= this.endBlocks.Count)
            {
                continue;
            }
            GameObject block = this.endBlocks[blockIdx];
            if (block == null)
            {
                continue;
            }
            RectTransform blockRect = block.gameObject.GetComponent<RectTransform>();
            float blockW = blockRect.sizeDelta.x;
            float blockH = blockRect.sizeDelta.y;
            string ids = "";
            CSLList<int> values = targetMap[blockIdx];
            int v = 0;
            while (v < values.Count)
            {
                if (v > 0)
                {
                    ids = ids + ",";
                }
                ids = ids + values[v];
                v = v + 1;
            }
            GameObject labelObj = new GameObject("TargetLabel");
            labelObj.transform.SetParent(block.transform, false);
            RectTransform rect = labelObj.gameObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(blockW * 0.8f, blockH * 0.8f);
            rect.anchoredPosition = new Vector2(0f, 0f);
            Text label = labelObj.gameObject.AddComponent<Text>();
            label.text = ids;
            label.fontSize = (int)(blockH * 0.35f);
            label.font = this.GetFont();
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(1f, 1f, 1f, 1f);
            label.raycastTarget = false;
        }
    }

    private void UpdateCollisionInfos()
    {
        bool activityDestroyed = GameObjectUtils.IsDestroyed(this.activityArea.gameObject);
        bool areaDestroyed = GameObjectUtils.IsDestroyed(this.activityArea.ActivityArea);
        if (activityDestroyed || areaDestroyed)
        {
            GLog.Debug("MarbleLotteryMachine::UpdateCollisionInfos", "activityArea 未赋值或已销毁");
            return;
        }
        RectTransform activityRectTransform = this.activityArea.ActivityArea;
        Vector2 activitySize = activityRectTransform.rect.size;
        Vector2 activityPivot = activityRectTransform.pivot;
        float xMin = -activitySize.x * activityPivot.x;
        float yMin = -activitySize.y * activityPivot.y;
        this.activityRect = new Rect(xMin, yMin, activitySize.x, activitySize.y);

        this.obstacleCollisionInfos.Clear();
        int i = 0;
        while (i < this.obstacles.Count)
        {
            GameObject obs = this.obstacles[i];
            if (obs == null)
            {
                i = i + 1;
                continue;
            }
            MarbleLotteryMachineObstacle comp = LuaBehaviour.getLuaBehaviour<MarbleLotteryMachineObstacle>(obs);
            RectTransform obsRect = obs.gameObject.GetComponent<RectTransform>();
            Vector2 worldPos = obsRect.position;
            Vector2 localPos = this.activityArea.ActivityArea.InverseTransformPoint(worldPos);
            float radius = this.obstacleRadius;
            bool compDestroyed = GameObjectUtils.IsDestroyed(comp.gameObject);
            bool areaDestroyed2 = GameObjectUtils.IsDestroyed(comp.CollisionArea);
            if (!compDestroyed && !areaDestroyed2)
            {
                RectTransform areaRect = comp.CollisionArea;
                radius = areaRect.rect.width * 0.5f;
                worldPos = areaRect.position;
                localPos = this.activityArea.ActivityArea.InverseTransformPoint(worldPos);
            }
            else
            {
                radius = obsRect.rect.width * 0.5f;
            }
            ObstacleCollisionInfo info = new ObstacleCollisionInfo();
            info.center = localPos;
            info.radius = radius;
            info.obj = obs;
            info.component = comp;
            this.obstacleCollisionInfos.Add(info);
            i = i + 1;
        }
        this.finalAreaCollisionInfos.Clear();
        int j = 0;
        while (j < this.endBlocks.Count)
        {
            GameObject area = this.endBlocks[j];
            if (area == null)
            {
                j = j + 1;
                continue;
            }
            MarbleLotteryMachineFinalArea comp = LuaBehaviour.getLuaBehaviour<MarbleLotteryMachineFinalArea>(area);
            RectTransform areaRect = area.gameObject.GetComponent<RectTransform>();
            Vector2 worldPos = areaRect.position;
            Vector2 areaSize = areaRect.rect.size;
            Vector2 localPos = this.activityArea.ActivityArea.InverseTransformPoint(worldPos);
            Rect localRect = new Rect(localPos.x - areaSize.x * 0.5f, localPos.y - areaSize.y * 0.5f, areaSize.x, areaSize.y);
            bool compDestroyed = GameObjectUtils.IsDestroyed(comp.gameObject);
            bool areaDestroyed2 = GameObjectUtils.IsDestroyed(comp.CollisionArea);
            if (!compDestroyed && !areaDestroyed2)
            {
                RectTransform compArea = comp.CollisionArea;
                worldPos = compArea.position;
                localPos = this.activityArea.ActivityArea.InverseTransformPoint(worldPos);
                Vector2 compSize = compArea.rect.size;
                localRect = new Rect(localPos.x - compSize.x * 0.5f, localPos.y - compSize.y * 0.5f, compSize.x, compSize.y);
            }
            FinalAreaCollisionInfo info = new FinalAreaCollisionInfo();
            info.rect = localRect;
            info.obj = area;
            info.component = comp;
            this.finalAreaCollisionInfos.Add(info);
            j = j + 1;
        }
    }

    public void GeneratePath()
    {
        bool launcherDestroyed = GameObjectUtils.IsDestroyed(this.launcher.gameObject);
        bool launchPosDestroyed = GameObjectUtils.IsDestroyed(this.launcher.LaunchPos);
        if (launcherDestroyed || launchPosDestroyed)
        {
            GLog.Debug("MarbleLotteryMachine::GeneratePath", "launcher 未赋值或已销毁");
            return;
        }
        bool activityDestroyed = GameObjectUtils.IsDestroyed(this.activityArea.gameObject);
        if (activityDestroyed)
        {
            GLog.Debug("MarbleLotteryMachine::GeneratePath", "activityArea 未赋值或已销毁");
            return;
        }
        bool trueEndValid = (this.trueEndIndex >= 0);
        if (trueEndValid)
        {
            trueEndValid = (this.trueEndIndex < this.blockCount);
        }
        if (!trueEndValid)
        {
            this.trueEndIndex = 0;
            this.SelectTrueEnd(0);
            this.SetInfoText("未选择目标，自动切换至目标 1");
        }
        Vector2 worldPos = this.launcher.LaunchPos.position;
        this.startPoint = this.activityArea.ActivityArea.InverseTransformPoint(worldPos);
        if (this.pathGenStopwatch == null)
        {
            this.pathGenStopwatch = new Stopwatch();
        }
        this.pathGenStopwatch.Reset();
        this.pathGenStopwatch.Start();
        this.ClearPath();

        PathResult result = this.GenerateValidPath(this.trueEndIndex, this.ballRadius);
        this.pathGenStopwatch.Stop();

        if (result.Success)
        {
            this.pathPoints.Clear();
            this.pathPoints.AddRange(result.Path);
            this.lastCollisionPoints.Clear();
            this.lastCollisionRecords = result.Records;
            this.GenerateBezierPath();
            this.DrawPath();
            float seconds = (float)this.pathGenStopwatch.Elapsed.TotalSeconds;
            this.SetInfoText("✅ 成功生成路径！尝试次数: " + this.pathGenAttempts + "，耗时: " + seconds + " 秒");
            GLog.Debug("MarbleLotteryMachine::GeneratePath", "✅ 成功生成路径！尝试次数: " + this.pathGenAttempts + "，耗时: " + seconds + " 秒");
        }
        else
        {
            this.SetInfoText("❌ 生成失败：尝试 " + this.pathGenAttempts + " 次");
            GLog.Debug("MarbleLotteryMachine::GeneratePath", "❌ 生成失败：尝试 " + this.pathGenAttempts + " 次");
        }
    }

    public PathResult GenerateValidPath(int targetIndex, float ballRadius)
    {
        int maxAttempts = 10000;
        int attempts = 0;
        while (attempts < maxAttempts)
        {
            attempts = attempts + 1;
            PathResult physResult = this.GeneratePhysicalPath(targetIndex);
            CSLList<Vector2> path = physResult.Path;
            if (path == null)
            {
                continue;
            }
            if (path.Count < 2)
            {
                continue;
            }
            Vector2 finalPos = path[path.Count - 1];
            int hitEnd = this.CheckHitEnd(finalPos);
            if (hitEnd == targetIndex)
            {
                this.pathGenAttempts = attempts;
                return new PathResult { Success = true, Path = path, Records = physResult.Records };
            }
        }
        return new PathResult { Success = false, Path = new CSLList<Vector2>(), Records = new CSLList<CollisionRecord>() };
    }

    private PathResult GeneratePhysicalPath(int targetIndex)
    {
        if (this.finalAreaCollisionInfos == null)
        {
            return new PathResult { Success = false, Path = new CSLList<Vector2>(), Records = new CSLList<CollisionRecord>() };
        }
        bool indexValid = (targetIndex >= 0);
        if (indexValid)
        {
            indexValid = (targetIndex < this.finalAreaCollisionInfos.Count);
        }
        if (!indexValid)
        {
            return new PathResult { Success = false, Path = new CSLList<Vector2>(), Records = new CSLList<CollisionRecord>() };
        }

        Rect targetRect = this.finalAreaCollisionInfos[targetIndex].rect;
        float targetTop = targetRect.yMax;
        float targetLeft = targetRect.xMin;
        float targetRight = targetRect.xMax;
        Vector2 pos = this.startPoint;
        float dt = 0.02f;
        int steps = 3000;
        float minDist = this.pathSamplingDistance;
        float ballRadius = this.ballRadius;

        Vector2 targetCenter = targetRect.center;
        Vector2 dirToTarget = (targetCenter - this.startPoint).normalized;
        float angleOffset = Random.Range(-30f, 30f) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(
            dirToTarget.x * Mathf.Cos(angleOffset) - dirToTarget.y * Mathf.Sin(angleOffset),
            dirToTarget.x * Mathf.Sin(angleOffset) + dirToTarget.y * Mathf.Cos(angleOffset)
        );
        if (dir.y >= 0f)
        {
            dir.y = -0.01f;
        }
        dir.Normalize();
        Vector2 vel = dir * this.initialSpeed;

        CSLList<Vector2> path = new CSLList<Vector2>();
        path.Add(pos);

        CSLList<float> recordedDistances = new CSLList<float>();
        CSLList<int> recordedIndices = new CSLList<int>();
        float cumulativeDistance = 0f;
        Vector2 lastPos = pos;

        int step = 0;
        while (step < steps)
        {
            vel.y = vel.y - this.gravity * dt;
            Vector2 newPos = new Vector2(pos.x + vel.x * dt, pos.y + vel.y * dt);
            float moveDist = MarbleLotteryMachine.Distance(lastPos, newPos);
            cumulativeDistance = cumulativeDistance + moveDist;
            lastPos = newPos;

            bool hitObs = false;
            int obsIdx = 0;
            while (obsIdx < this.obstacleCollisionInfos.Count)
            {
                ObstacleCollisionInfo info = this.obstacleCollisionInfos[obsIdx];
                Vector2 toBall = newPos - info.center;
                float dist = toBall.magnitude;
                if (dist < ballRadius + info.radius)
                {
                    if (dist > 0.001f)
                    {
                        hitObs = true;
                        Vector2 normal = toBall.normalized;
                        recordedDistances.Add(cumulativeDistance);
                        recordedIndices.Add(obsIdx);

                        newPos = info.center + normal * (ballRadius + info.radius);
                        float velAlongNormal = Vector2.Dot(vel, normal);
                        if (velAlongNormal < 0f)
                        {
                            vel = (vel - normal * 2f * velAlongNormal) * this.bounceFactor;
                        }
                        break;
                    }
                }
                obsIdx = obsIdx + 1;
            }

            if (newPos.x - ballRadius < this.activityRect.xMin)
            {
                recordedDistances.Add(cumulativeDistance);
                recordedIndices.Add(-1);
                newPos.x = this.activityRect.xMin + ballRadius;
                if (vel.x < 0f)
                {
                    vel.x = vel.x * -this.bounceFactor;
                }
            }
            if (newPos.x + ballRadius > this.activityRect.xMax)
            {
                recordedDistances.Add(cumulativeDistance);
                recordedIndices.Add(-1);
                newPos.x = this.activityRect.xMax - ballRadius;
                if (vel.x > 0f)
                {
                    vel.x = vel.x * -this.bounceFactor;
                }
            }
            if (newPos.y + ballRadius > this.activityRect.yMax)
            {
                recordedDistances.Add(cumulativeDistance);
                recordedIndices.Add(-1);
                newPos.y = this.activityRect.yMax - ballRadius;
                if (vel.y > 0f)
                {
                    vel.y = -Mathf.Abs(vel.y) * 0.2f;
                }
                else
                {
                    vel.y = -1f;
                }
            }
            if (newPos.y - ballRadius < this.activityRect.yMin)
            {
                recordedDistances.Add(cumulativeDistance);
                recordedIndices.Add(-1);
                newPos.y = this.activityRect.yMin + ballRadius;
                if (vel.y < 0f)
                {
                    vel.y = vel.y * -this.bounceFactor;
                }
                path.Add(newPos);
                break;
            }

            if (newPos.x >= targetLeft)
            {
                if (newPos.x <= targetRight)
                {
                    if (newPos.y >= targetTop - ballRadius)
                    {
                        if (newPos.y <= targetTop + ballRadius)
                        {
                            path.Add(newPos);
                            break;
                        }
                    }
                }
            }

            pos = newPos;
            bool validPos = this.IsValidVector2(pos);
            if (validPos)
            {
                float distFromLast = MarbleLotteryMachine.Distance(pos, path[path.Count - 1]);
                if (distFromLast >= minDist)
                {
                    path.Add(pos);
                }
            }

            if (vel.magnitude < 1f)
            {
                if (pos.y - ballRadius < this.activityRect.yMin + 30f)
                {
                    break;
                }
            }
            if (pos.y > this.activityRect.yMax + 100f)
            {
                break;
            }
            if (pos.y < this.activityRect.yMin - 100f)
            {
                break;
            }

            step = step + 1;
        }

        if (path.Count < 2)
        {
            path.Add(new Vector2(this.startPoint.x, this.activityRect.yMin + ballRadius));
        }

        float totalDistance = cumulativeDistance;
        if (totalDistance <= 0.001f)
        {
            totalDistance = 1f;
        }

        CSLList<CollisionRecord> records = new CSLList<CollisionRecord>();
        int i = 0;
        while (i < recordedDistances.Count)
        {
            float progressFloat = Mathf.Clamp01(recordedDistances[i] / totalDistance);
            int progressTenThousandth = ToInt(progressFloat * 10000f);
            if (progressTenThousandth < 0)
            {
                progressTenThousandth = 0;
            }
            if (progressTenThousandth > 10000)
            {
                progressTenThousandth = 10000;
            }
            records.Add(new CollisionRecord { progress = progressTenThousandth, obstacleIndex = recordedIndices[i] });
            i = i + 1;
        }

        return new PathResult { Success = true, Path = path, Records = records };
    }

    private int CheckHitEnd(Vector2 localPos)
    {
        if (this.finalAreaCollisionInfos == null)
        {
            return -1;
        }
        int i = 0;
        while (i < this.finalAreaCollisionInfos.Count)
        {
            Rect rect = this.finalAreaCollisionInfos[i].rect;
            float targetTop = rect.yMax;
            float targetLeft = rect.xMin;
            float targetRight = rect.xMax;
            bool xHit = (localPos.x >= targetLeft);
            if (xHit)
            {
                xHit = (localPos.x <= targetRight);
            }
            if (xHit)
            {
                bool yHit = (localPos.y >= targetTop - this.ballRadius);
                if (yHit)
                {
                    yHit = (localPos.y <= targetTop + this.ballRadius);
                }
                if (yHit)
                {
                    return i;
                }
            }
            i = i + 1;
        }
        return -1;
    }

    private void GenerateBezierPath()
    {
        if (this.pathPoints.Count < 2)
        {
            return;
        }
        this.bezierPoints.Clear();
        int segments = this.bezierSegments;
        int i = 0;
        while (i < this.pathPoints.Count - 1)
        {
            Vector2 p0 = this.pathPoints[i];
            Vector2 p1 = this.pathPoints[i + 1];
            Vector2 dir = (p1 - p0).normalized;
            float dist = MarbleLotteryMachine.Distance(p0, p1);
            Vector2 cp1 = p0 + dir * 0.3f * dist;
            Vector2 cp2 = p1 - dir * 0.3f * dist;
            int j = 0;
            while (j <= segments)
            {
                float t = j / (float)segments;
                this.bezierPoints.Add(this.BezierCurve(p0, cp1, cp2, p1, t));
                j = j + 1;
            }
            i = i + 1;
        }
        this.finalPath = new CSLList<Vector2>(this.bezierPoints);
        if (this.finalPath.Count > 0)
        {
            this.finalPath[0] = this.startPoint;
        }

        // ---- 压缩路径点数到 256 以内 ----
        int maxPoints = 256;
        if (this.finalPath.Count > maxPoints)
        {
            CSLList<Vector2> compressed = new CSLList<Vector2>();
            float step = 1f / (maxPoints - 1);
            int idx = 0;
            while (idx < maxPoints)
            {
                float t = idx * step;
                if (t > 1f)
                {
                    t = 1f;
                }
                Vector2 point = this.GetPointOnPath(this.finalPath, t);
                compressed.Add(point);
                idx = idx + 1;
            }
            // 确保终点精确匹配
            if (compressed.Count > 0)
            {
                compressed[compressed.Count - 1] = this.finalPath[this.finalPath.Count - 1];
            }
            this.finalPath = compressed;
        }
    }

    private Vector2 BezierCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float u = 1f - t;
        float x = p0.x * u * u * u + p1.x * 3f * u * u * t + p2.x * 3f * u * t * t + p3.x * t * t * t;
        float y = p0.y * u * u * u + p1.y * 3f * u * u * t + p2.y * 3f * u * t * t + p3.y * t * t * t;
        return new Vector2(x, y);
    }

    private void DrawPath()
    {
        if (this.gameContainer == null)
        {
            GLog.Debug("MarbleLotteryMachine::DrawPath", "gameContainer 未赋值");
            return;
        }
        if (this.pathLine == null)
        {
            GameObject lineObj = new GameObject("PathLine");
            lineObj.transform.SetParent(this.gameContainer.transform, false);
            this.pathLine = lineObj.gameObject.AddComponent<LineRenderer>();
            this.pathLine.material = new Material(Shader.Find("Sprites/Default"));
            this.pathLine.startColor = new Color(0f, 1f, 1f, 1f);
            this.pathLine.endColor = new Color(0f, 1f, 1f, 1f);
            this.pathLine.useWorldSpace = true;
            this.pathLine.positionCount = 0;
            this.pathLine.sortingOrder = 3000;
            this.pathLine.startWidth = 0.03f;
            this.pathLine.endWidth = 0.03f;
        }
        if (this.finalPath == null)
        {
            this.pathLine.positionCount = 0;
            return;
        }
        if (this.finalPath.Count < 2)
        {
            this.pathLine.positionCount = 0;
            return;
        }
        bool activityDestroyed = GameObjectUtils.IsDestroyed(this.activityArea.gameObject);
        if (activityDestroyed)
        {
            return;
        }
        this.pathLine.positionCount = this.finalPath.Count;
        int i = 0;
        while (i < this.finalPath.Count)
        {
            Vector2 worldPos = this.activityArea.ActivityArea.TransformPoint(this.finalPath[i]);
            this.pathLine.SetPosition(i, new Vector3(worldPos.x, worldPos.y, 0f));
            i = i + 1;
        }
    }

    private void DrawCollisionMarkers(CSLList<Vector2> points)
    {
        int i = 0;
        while (i < this.collisionMarkers.Count)
        {
            GameObject marker = this.collisionMarkers[i];
            if (marker != null)
            {
                GameObject.Destroy(marker);
            }
            i = i + 1;
        }
        this.collisionMarkers.Clear();
    }

    public void ClearPath()
    {
        if (this.pathLine != null)
        {
            this.pathLine.positionCount = 0;
        }
        this.finalPath.Clear();
        int i = 0;
        while (i < this.collisionMarkers.Count)
        {
            GameObject marker = this.collisionMarkers[i];
            if (marker != null)
            {
                GameObject.Destroy(marker);
            }
            i = i + 1;
        }
        this.collisionMarkers.Clear();
        this.lastCollisionPoints.Clear();
        this.lastCollisionRecords.Clear();
    }

    public void LaunchBall()
{
    // 1. 前置检查
    bool activityDestroyed = GameObjectUtils.IsDestroyed(this.activityArea.gameObject);
    if (activityDestroyed)
    {
        GLog.Debug("MarbleLotteryMachine::LaunchBall", "activityArea 未赋值或已销毁");
        return;
    }
    this.UpdateCollisionInfos();

    if (this.finalPath == null || this.finalPath.Count < 2)
    {
        GLog.Debug("MarbleLotteryMachine::LaunchBall", "缺少有效路径");
        return;
    }

    bool launcherDestroyed = GameObjectUtils.IsDestroyed(this.launcher.gameObject);
    bool launchPosDestroyed = GameObjectUtils.IsDestroyed(this.launcher.LaunchPos);
    if (launcherDestroyed || launchPosDestroyed)
    {
        GLog.Debug("MarbleLotteryMachine::LaunchBall", "launcher.LaunchPos 未赋值或已销毁");
        return;
    }

    bool containerDestroyed = GameObjectUtils.IsDestroyed(this.ballContainer);
    if (containerDestroyed)
    {
        GLog.Debug("MarbleLotteryMachine::LaunchBall", "ballContainer 未赋值或已销毁");
        return;
    }

    // 2. 使用原始路径（完全不做偏移）
    CSLList<Vector2> usedPath = new CSLList<Vector2>(this.finalPath); // 复制一份供小球使用

    // 3. 计算路径起点在世界坐标中的位置
    Vector2 startLocalPos = usedPath[0];                         // activityArea 局部坐标
    Vector2 startWorldPos = this.activityArea.ActivityArea.TransformPoint(startLocalPos);

    // 4. 将起点转换到 ballContainer 的局部坐标（因为小球将放在 ballContainer 下）
    Vector2 containerLocalPos = this.ballContainer.InverseTransformPoint(startWorldPos);

    // 5. 获取或创建小球对象
    GameObject ballObj = this.GetOrCreateBallObject();
    if (ballObj == null)
    {
        GLog.Debug("MarbleLotteryMachine::LaunchBall", "获取小球对象失败");
        return;
    }
    ballObj.transform.SetParent(this.ballContainer, false);
    ballObj.SetActive(true);
    RectTransform rect = ballObj.gameObject.GetComponent<RectTransform>();
    rect.localPosition = new Vector3(containerLocalPos.x, containerLocalPos.y, 0f);

    // 6. 获取小球组件和半径
    MarbleLotteryMachineBall ballComp = LuaBehaviour.getLuaBehaviour<MarbleLotteryMachineBall>(ballObj);
    float radius = this.GetBallRadius(ballObj);

    // 7. 创建 BallInstance
    BallInstance newBall = new BallInstance();
    this.ballCounter++;
    newBall.ballID = this.ballCounter;
    newBall.expectTargetIndex = this.trueEndIndex;
    newBall.path = usedPath;                                   // 使用原始路径副本
    newBall.progress = 0f;
    // 计算总长度
    float totalLen = 0f;
    for (int j = 0; j < usedPath.Count - 1; j++)
        totalLen += MarbleLotteryMachine.Distance(usedPath[j], usedPath[j + 1]);
    newBall.totalLength = totalLen;
    newBall.moveSpeed = this.initialSpeed * 1.5f;
    newBall.isActive = true;
    newBall.obj = ballObj;
    newBall.rect = rect;
    newBall.ballComponent = ballComp;
    newBall.radius = radius;

    // 8. 碰撞记录（与原始路径对应，不需要偏移）
    newBall.collisionRecords.Clear();
    foreach (CollisionRecord src in this.lastCollisionRecords)
        newBall.collisionRecords.Add(new CollisionRecord { progress = src.progress, obstacleIndex = src.obstacleIndex });
    newBall.triggeredIndex = 0;
    newBall.triggeredRecordIndices = new CSLList<int>();

#if MARBLE_DEV
    newBall.label = this.CreateLabel(ballObj, newBall.ballID + "→" + this.trueEndIndex);
    if (newBall.label != null)
        newBall.label.color = new Color(0f, 0f, 0f, 1f);
#endif

    this.balls.Add(newBall);

    // 9. 设置发射器旋转（基于路径方向，仅调整朝向，不影响路径显示）
    if (usedPath.Count >= 2)
    {
        Vector2 dir = (usedPath[1] - usedPath[0]).normalized;
        float rotAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        this.launcher.transform.localEulerAngles = new Vector3(0f, 0f, rotAngle);
    }

    // 10. 更新界面信息（不调用 DrawPath()，不修改 finalPath）
    this.SetInfoText("发射路径小球 #" + newBall.ballID + " → 目标 " + this.trueEndIndex + "，当前小球总数 " + this.balls.Count);
    GLog.Debug("MarbleLotteryMachine::LaunchBall", "LaunchBall: 使用原始路径，点数=" + usedPath.Count + ", 总长=" + totalLen + ", 碰撞记录=" + newBall.collisionRecords.Count);
}

#if MARBLE_DEV
    private Text CreateLabel(GameObject parent, string text)
    {
        if (parent == null)
        {
            return null;
        }
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(parent.transform, false);
        RectTransform rect = labelObj.gameObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.sizeDelta = new Vector2(0f, 0f);
        Text label = labelObj.gameObject.AddComponent<Text>();
        label.text = text;
        label.fontSize = 11;
        label.font = this.GetFont();
        label.alignment = TextAnchor.MiddleCenter;
        label.color = new Color(0f, 0f, 0f, 1f);
        label.raycastTarget = false;
        return label;
    }
#endif

    private Font GetFont()
    {
        return Font.CreateDynamicFontFromOSFont("Arial", 14);
    }
}