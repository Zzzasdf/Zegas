using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using TKLua;
using Random = UnityEngine.Random;

[Serializable]
public class SerializableCollisionRecord { public Vector2 position; public int obstacleIndex; }
[Serializable]
public class SerializablePathData { public Vector2[] finalPath; public int trueEndIndex; public SerializableCollisionRecord[] collisionRecords; }
[Serializable]
public class SlotData { public SerializablePathData[] paths; }
[Serializable]
public class AllPathsData { public SlotData[] slots; }

[CSLStub.Out("Activity/Marble")]
public partial class MarbleLotteryMachine : CSLuaBehaviour
{
    // ---- 序列化字段 ----
    [SerializeField] [Bind("MarbleLotteryPaths")] protected TextAsset marbleLotteryPaths;
    [SerializeField] [Bind("ballTemplate")] protected MarbleLotteryMachineBall ballTemplate;
    [SerializeField] [Bind("obstacleTemplate")] protected MarbleLotteryMachineObstacle obstacleTemplate;
    [SerializeField] [Bind("finalAreaTemplate")] protected MarbleLotteryMachineFinalArea finalAreaTemplate;
    [SerializeField] [Bind("launcher")] protected MarbleLotteryMachineLauncher launcher;
    [SerializeField] [Bind("activityArea")] protected MarbleLotteryMachineActivityArea activityArea;
    [SerializeField] [Bind("obstacleContainer")] protected RectTransform obstacleContainer;
    [SerializeField] [Bind("finalAreaContainer")] protected RectTransform finalAreaContainer;
    [SerializeField] [Bind("ballContainer")] protected RectTransform ballContainer;

    // ---- UI 按钮 ----
    public Button launchBtn, resetBtn, regenerateBtn, generatePathBtn, directLaunchBtn;
    public Text infoText;
    public Text runStatusText;

    // ---- 物理参数 ----
    public float gravity = 600f;
    public float bounceFactor = 0.8f;
    public float initialSpeed = 400f;
    public float ballRadius = 15f;
    public float obstacleRadius = 25f;
    public float horizontalMargin = 0.15f;
    public float verticalMargin = 0.05f;
    public float blockHeight = 60f;
    public int blockCount = 8;
    public Color trueEndColor = Color.green;

    // ---- 时间参数 ----
    public float recycleDelay = 2f;
    public float launchInterval = 0.5f;
    public float rotateSpeed = 180f;

    // ---- 场景元素 ----
    public CSLList<GameObject> obstacles = new CSLList<GameObject>();
    public CSLList<CSLList<GameObject>> layerObstacles = new CSLList<CSLList<GameObject>>();
    public CSLList<GameObject> endBlocks = new CSLList<GameObject>();
    public int trueEndIndex = -1;
    public Vector2 startPoint, endPoint;
    public Rect activityRect;
    public float blockStartY, blockWidth;
    public GameObject gameContainer;
    public Canvas mainCanvas;

    // ---- 路径相关 ----
    public CSLList<Vector2> pathPoints = new CSLList<Vector2>();
    public CSLList<Vector2> bezierPoints = new CSLList<Vector2>();
    public CSLList<Vector2> finalPath = new CSLList<Vector2>();
    public LineRenderer pathLine;
    public CSLList<GameObject> collisionMarkers = new CSLList<GameObject>();
    public int collisionCounter = 0;

    // ---- 碰撞记录 ----
    [CSLStub.Out("Activity/Marble")]
    public class CollisionRecord { public Vector2 position; public int obstacleIndex; }

    // ---- 小球实例 ----
    [CSLStub.Out("Activity/Marble")]
    public class BallInstance
    {
        public GameObject obj;
        public RectTransform rect;
        public Text label;
        public CSLList<Vector2> path;
        public float progress = 0f;
        public float totalLength = 0f;
        public float moveSpeed = 300f;
        public Vector2 velocity;
        public bool isActive = false;
        public bool isComplete = false;
        public bool isRecycled = false;
        public float recycleProgress = 0f;
        public Vector3 recycleStartScale;
        public bool isRecycling = false;
        public int ballID;
        public int expectTargetIndex;
        public int actualHitIndex = -1;
        public int triggeredIndex = 0;
        public int batchId = -1;
        public bool success = false;
        public MarbleLotteryMachineBall ballComponent;
        public float radius;
        public CSLList<CollisionRecord> collisionRecords = new CSLList<CollisionRecord>();
        public float completeTime;
    }

    public CSLList<BallInstance> balls = new CSLList<BallInstance>();
    public int ballCounter = 0;

    // ---- 对象池 ----
    private Queue<GameObject> ballPool = new Queue<GameObject>();

    // ---- 路径生成统计 ----
    public int pathGenAttempts = 0;
    protected Stopwatch pathGenStopwatch;
    protected bool isPathGenRecording = false;
    public CSLList<Vector2> lastCollisionPoints = new CSLList<Vector2>();
    private CSLList<CollisionRecord> lastCollisionRecords = new CSLList<CollisionRecord>();

    // ---- 碰撞信息缓存 ----
    [CSLStub.Out("Activity/Marble")]
    protected class ObstacleCollisionInfo { public Vector2 center; public float radius; public GameObject obj; public MarbleLotteryMachineObstacle component; }
    [CSLStub.Out("Activity/Marble")]
    protected class FinalAreaCollisionInfo { public Rect rect; public GameObject obj; public MarbleLotteryMachineFinalArea component; }
    protected CSLList<ObstacleCollisionInfo> obstacleCollisionInfos = new CSLList<ObstacleCollisionInfo>();
    protected CSLList<FinalAreaCollisionInfo> finalAreaCollisionInfos = new CSLList<FinalAreaCollisionInfo>();

    // ---- 路径结果 ----
    [CSLStub.Out("Activity/Marble")]
    public class PathResult { public bool Success; public CSLList<Vector2> Path; public CSLList<CollisionRecord> Records; }

    // ---- 保存路径数据 ----
    [CSLStub.Out("Activity/Marble")]
    public class SavedPathData
    {
        public CSLList<Vector2> finalPath = new CSLList<Vector2>();
        public int trueEndIndex = -1;
        public CSLList<CollisionRecord> collisionRecords = new CSLList<CollisionRecord>();
        public bool IsEmpty => finalPath == null || finalPath.Count < 2;
    }

    public CSLList<SavedPathData>[] savedPaths = new CSLList<SavedPathData>[8];
    public int selectedSlot = -1;

    // ---- UI 控件引用 ----
    protected Button[] slotBtns = new Button[8];
    protected Text slotInfoText;
    protected Button saveBtn, savePersistentBtn;
    protected Transform pathListContent;
    protected CSLList<GameObject> pathEntryObjects = new CSLList<GameObject>();
    private CSLList<float> targetXInObstacle = new CSLList<float>();
    private CSLList<Button> targetSelectButtons = new CSLList<Button>();

    // ---- Run 功能 ----
    public bool IsRunning { get; private set; } = false;
    private Action onAllCompleteCallback;
    private int currentBatchId = 0;
    private int totalBallsCount = 0;
    private bool batchCompletedNotified = false;
    private Quaternion launcherInitialRotation;
    private Coroutine runCoroutine = null;

    // ============================== 生命周期 ==============================
    public void SetStatusText(string text, Color color) => Debug.Log("[Status] " + text);
    public void SetInfoText(string text) { if (infoText != null) infoText.text = text; }
    public Canvas GetMainCanvas() => mainCanvas;
    public Transform GetGameContainer() => gameContainer != null ? gameContainer.transform : null;

    void Awake()
    {
        for (int i = 0; i < savedPaths.Length; i++) savedPaths[i] = new CSLList<SavedPathData>();
        CreateUI();
        CreateSceneElements();
        pathGenStopwatch = new Stopwatch();
        lastCollisionPoints = new CSLList<Vector2>();
        LoadPathsFromJson();
        if (launcher != null) launcherInitialRotation = launcher.transform.localRotation;
    }

    void Start()
    {
        if (launchBtn != null) launchBtn.onClick.AddListener(LaunchBall);
        if (resetBtn != null) resetBtn.onClick.AddListener(ResetAll);
        if (regenerateBtn != null) regenerateBtn.onClick.AddListener(RegenerateScene);
        if (generatePathBtn != null) generatePathBtn.onClick.AddListener(GeneratePath);
        if (directLaunchBtn != null) directLaunchBtn.onClick.AddListener(DirectLaunch);
        if (saveBtn != null) saveBtn.onClick.AddListener(SaveCurrentPath);
        if (savePersistentBtn != null) savePersistentBtn.onClick.AddListener(() => { SavePathsToJson(); SetInfoText("已保存所有路径到持久化目录"); });
        UpdateCollisionInfos();
        SelectSlot(0);
    }

    void Update()
    {
        UpdateBalls();
        CheckBatchComplete();
        RecycleBalls();
        UpdateRecycleAnimations();
        UpdateRunStatus();
    }

    // ============================== 重置与重生 ==============================
    public void ResetAll()
    {
        if (runCoroutine != null) { StopCoroutine(runCoroutine); runCoroutine = null; }
        foreach (var ball in balls) if (ball.obj != null) GameObject.Destroy(ball.obj);
        balls.Clear();
        while (ballPool.Count > 0) { var obj = ballPool.Dequeue(); if (obj != null) GameObject.Destroy(obj); }
        ballCounter = 0;
        ClearPath();
        if (isPathGenRecording && pathGenStopwatch != null) { pathGenStopwatch.Stop(); isPathGenRecording = false; }
        SetInfoText("已重置");
        IsRunning = false;
        onAllCompleteCallback = null;
        batchCompletedNotified = false;
        if (launcher != null) launcher.transform.localRotation = launcherInitialRotation;
    }

    public void RegenerateScene()
    {
        ResetAll();
        GenerateObstacles();
        GenerateEndBlocks();
        ClearPath();
        pathGenAttempts = 0;
        if (isPathGenRecording && pathGenStopwatch != null) { pathGenStopwatch.Stop(); isPathGenRecording = false; }
        UpdateCollisionInfos();
        SetInfoText("场景已重生，点击生成路径或发射");
    }

    // ============================== 工具方法 ==============================
    protected bool CircleIntersect(Vector2 c1, float r1, Vector2 c2, float r2) => Distance(c1, c2) < r1 + r2;
    protected bool CircleRectIntersect(Vector2 center, float radius, Rect rect)
    {
        float closestX = Mathf.Clamp(center.x, rect.xMin, rect.xMax);
        float closestY = Mathf.Clamp(center.y, rect.yMin, rect.yMax);
        return Distance(center, new Vector2(closestX, closestY)) < radius;
    }
    protected float GetBallRadius(GameObject ballObj)
    {
        MarbleLotteryMachineBall comp = ballObj.GetComponent<MarbleLotteryMachineBall>();
        if (comp != null && comp.CollisionArea != null) return comp.CollisionArea.rect.width * 0.5f;
        return 15f;
    }
    public static float Distance(Vector2 a, Vector2 b) => (float)Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
    public static Vector2 Lerp(Vector2 a, Vector2 b, float t) => new Vector2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);

    // ============================== JSON 序列化 ==============================
    private void LoadPathsFromJson()
    {
        if (marbleLotteryPaths == null) return;
        string json = marbleLotteryPaths.text;
        if (string.IsNullOrEmpty(json)) return;
        try
        {
            AllPathsData data = JsonUtility.FromJson<AllPathsData>(json);
            if (data == null || data.slots == null) return;
            for (int slot = 0; slot < data.slots.Length && slot < savedPaths.Length; slot++)
            {
                savedPaths[slot].Clear();
                if (data.slots[slot] == null || data.slots[slot].paths == null) continue;
                foreach (var pathData in data.slots[slot].paths)
                {
                    SavedPathData spd = new SavedPathData();
                    spd.trueEndIndex = pathData.trueEndIndex;
                    spd.finalPath = new CSLList<Vector2>(pathData.finalPath);
                    spd.collisionRecords = new CSLList<CollisionRecord>();
                    if (pathData.collisionRecords != null)
                        foreach (var rec in pathData.collisionRecords)
                            spd.collisionRecords.Add(new CollisionRecord { position = rec.position, obstacleIndex = rec.obstacleIndex });
                    savedPaths[slot].Add(spd);
                }
            }
            RefreshPathList();
            UpdateSlotInfo();
            Debug.Log("路径加载成功");
        }
        catch (Exception e) { Debug.LogError("加载路径 JSON 失败: " + e.Message); }
    }

    private void SavePathsToJson()
    {
        int totalPaths = 0;
        for (int i = 0; i < savedPaths.Length; i++) totalPaths += savedPaths[i].Count;
        if (totalPaths == 0) { Debug.LogWarning("没有路径可保存，跳过写入"); return; }

        AllPathsData data = new AllPathsData();
        data.slots = new SlotData[savedPaths.Length];
        for (int slot = 0; slot < savedPaths.Length; slot++)
        {
            var list = savedPaths[slot];
            data.slots[slot] = new SlotData();
            data.slots[slot].paths = new SerializablePathData[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                var src = list[i];
                SerializablePathData dest = new SerializablePathData();
                dest.trueEndIndex = src.trueEndIndex;
                dest.finalPath = new Vector2[src.finalPath.Count];
                for (int j = 0; j < src.finalPath.Count; j++) dest.finalPath[j] = src.finalPath[j];
                dest.collisionRecords = new SerializableCollisionRecord[src.collisionRecords.Count];
                for (int j = 0; j < src.collisionRecords.Count; j++)
                    dest.collisionRecords[j] = new SerializableCollisionRecord { position = src.collisionRecords[j].position, obstacleIndex = src.collisionRecords[j].obstacleIndex };
                data.slots[slot].paths[i] = dest;
            }
        }
        string json = JsonUtility.ToJson(data, true);
        string path = Application.persistentDataPath + "/MarbleLotteryPaths.json";
        System.IO.File.WriteAllText(path, json);
        Debug.Log("路径已保存到: " + path);
    }

    public void ReplaceTextAsset()
    {
#if UNITY_EDITOR
        if (marbleLotteryPaths == null) { SetInfoText("错误：marbleLotteryPaths 未赋值"); return; }
        string persistentPath = Application.persistentDataPath + "/MarbleLotteryPaths.json";
        if (!System.IO.File.Exists(persistentPath)) { SetInfoText("持久化文件不存在，请先保存路径"); return; }
        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(marbleLotteryPaths);
        if (string.IsNullOrEmpty(assetPath)) { SetInfoText("无法获取 TextAsset 的路径"); return; }
        string json = System.IO.File.ReadAllText(persistentPath);
        System.IO.File.WriteAllText(assetPath, json);
        UnityEditor.AssetDatabase.Refresh();
        marbleLotteryPaths = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
        LoadPathsFromJson();
        SetInfoText("已成功替换 TextAsset 并重新加载");
        Debug.Log("Replaced TextAsset at: " + assetPath);
#else
        SetInfoText("此功能仅在 Unity Editor 中可用");
#endif
    }

    // ============================== 对象池 ==============================
    private GameObject GetOrCreateBallObject()
    {
        if (ballPool.Count > 0)
        {
            var obj = ballPool.Dequeue();
            if (obj != null) { obj.SetActive(true); obj.transform.localScale = Vector3.one; return obj; }
        }
        return GameObject.Instantiate(ballTemplate.gameObject);
    }

    // ============================== 回收逻辑（延迟+对象池） ==============================
    private void RecycleBalls()
    {
        for (int i = balls.Count - 1; i >= 0; i--)
        {
            BallInstance ball = balls[i];
            if (!ball.isComplete || ball.isRecycled || ball.isRecycling) continue;
            if (Time.time - ball.completeTime >= recycleDelay)
            {
                ball.isRecycling = true;
                ball.recycleProgress = 0f;
                ball.recycleStartScale = ball.obj.transform.localScale;
            }
        }
    }

    private void UpdateRecycleAnimations()
    {
        float speed = 2.5f;
        for (int i = balls.Count - 1; i >= 0; i--)
        {
            BallInstance ball = balls[i];
            if (!ball.isRecycling) continue;
            ball.recycleProgress += Time.deltaTime * speed;
            if (ball.recycleProgress >= 1f)
            {
                ball.recycleProgress = 1f;
                ball.obj.transform.localScale = Vector3.zero;
                ball.obj.SetActive(false);
                ballPool.Enqueue(ball.obj);
                ball.obj = null;
                ball.rect = null;
                ball.ballComponent = null;
                balls.RemoveAt(i);
            }
            else
            {
                float scale = 1f - ball.recycleProgress;
                ball.obj.transform.localScale = ball.recycleStartScale * scale;
            }
        }
    }

    // ============================== Run 功能（核心） ==============================
    public void Run(CSLList<int> targets, Action onComplete)
    {
        // ★ 立即检查并锁定状态
        if (IsRunning)
        {
            Debug.LogWarning("Machine is already running, cannot call Run again.");
            return;
        }
        if (targets == null || targets.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        if (runCoroutine != null) { StopCoroutine(runCoroutine); runCoroutine = null; }

        // ★ 立即设为运行中，阻止后续点击
        IsRunning = true;
        batchCompletedNotified = false;

        runCoroutine = StartCoroutine(RunCoroutine(targets, onComplete));
    }

    private System.Collections.IEnumerator RunCoroutine(CSLList<int> targets, Action onComplete)
    {
        // IsRunning 已在 Run 中设为 true，这里不再重复设置
        onAllCompleteCallback = onComplete;
        currentBatchId++;
        totalBallsCount = targets.Count;

        if (launcher != null && launcher.LaunchPos != null)
        {
            Vector2 worldPos = launcher.LaunchPos.position;
            startPoint = activityArea.ActivityArea.InverseTransformPoint(worldPos);
        }

        for (int i = 0; i < targets.Count; i++)
        {
            int target = targets[i];
            CSLList<Vector2> path = null;
            CSLList<CollisionRecord> collisionRecs = null;

            var slot = savedPaths[target];
            if (slot != null && slot.Count > 0)
            {
                int idx = Random.Range(0, slot.Count);
                var data = slot[idx];
                path = new CSLList<Vector2>(data.finalPath);
                collisionRecs = data.collisionRecords;
                if (path != null && path.Count > 0)
                {
                    Vector2 offset = startPoint - path[0];
                    for (int j = 0; j < path.Count; j++) path[j] += offset;
                }
            }
            else
            {
                var result = GeneratePathForTarget(target);
                if (result != null && result.Success) { path = result.Path; collisionRecs = result.Records; }
            }

            if (path == null || path.Count < 2)
            {
                Debug.LogError($"无法为目标 {target} 获取路径，跳过该小球");
                totalBallsCount--;
                continue;
            }

            // 平滑旋转 launcher
            if (path.Count >= 2 && launcher != null)
            {
                Vector2 dir = (path[1] - path[0]).normalized;
                float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
                Quaternion startRot = launcher.transform.localRotation;
                float duration = Mathf.Abs(Mathf.DeltaAngle(startRot.eulerAngles.z, targetAngle)) / rotateSpeed;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    launcher.transform.localRotation = Quaternion.Slerp(startRot, targetRotation, t);
                    yield return null;
                }
                launcher.transform.localRotation = targetRotation;
            }

            LaunchBallWithPath(path, target, collisionRecs);

            if (i < targets.Count - 1)
                yield return new WaitForSeconds(launchInterval);
        }

        // 回正
        if (launcher != null)
        {
            Quaternion startRot = launcher.transform.localRotation;
            float duration = Mathf.Abs(Mathf.DeltaAngle(startRot.eulerAngles.z, launcherInitialRotation.eulerAngles.z)) / rotateSpeed;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                launcher.transform.localRotation = Quaternion.Slerp(startRot, launcherInitialRotation, t);
                yield return null;
            }
            launcher.transform.localRotation = launcherInitialRotation;
        }

        runCoroutine = null;

        if (totalBallsCount == 0)
        {
            IsRunning = false;
            onAllCompleteCallback?.Invoke();
            onAllCompleteCallback = null;
            batchCompletedNotified = true;
        }
    }

    public PathResult GeneratePathForTarget(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= finalAreaCollisionInfos.Count) return null;
        int oldTrueEnd = trueEndIndex;
        var result = GenerateValidPath(targetIndex, ballRadius);
        trueEndIndex = oldTrueEnd;
        return result;
    }

    private void LaunchBallWithPath(CSLList<Vector2> path, int targetIndex, CSLList<CollisionRecord> collisionRecords = null)
    {
        if (path == null || path.Count < 2) { Debug.LogError("路径无效，无法发射"); return; }
        if (launcher == null || launcher.LaunchPos == null) { Debug.LogError("launcher 未赋值"); return; }
        if (ballTemplate == null || ballContainer == null) { Debug.LogError("ballTemplate 或 ballContainer 未赋值"); return; }

        Vector2 worldPos = launcher.LaunchPos.position;
        Vector2 containerLocalPos = ballContainer.InverseTransformPoint(worldPos);

        GameObject ballObj = GetOrCreateBallObject();
        ballObj.transform.SetParent(ballContainer, false);
        ballObj.SetActive(true);
        RectTransform rect = ballObj.GetComponent<RectTransform>();
        rect.localPosition = new Vector3(containerLocalPos.x, containerLocalPos.y, 0);

        MarbleLotteryMachineBall ballComp = ballObj.GetComponent<MarbleLotteryMachineBall>();
        float radius = GetBallRadius(ballObj);

        BallInstance newBall = new BallInstance();
        ballCounter++;
        newBall.ballID = ballCounter;
        newBall.expectTargetIndex = targetIndex;
        newBall.path = path;
        newBall.progress = 0f;
        newBall.totalLength = 0f;
        for (int i = 0; i < path.Count - 1; i++) newBall.totalLength += Distance(path[i], path[i + 1]);
        newBall.moveSpeed = initialSpeed * 1.5f;
        newBall.isActive = true;
        newBall.obj = ballObj;
        newBall.rect = rect;
        newBall.ballComponent = ballComp;
        newBall.radius = radius;
        newBall.batchId = currentBatchId;

        newBall.label = CreateLabel(ballObj, newBall.ballID + "→" + targetIndex);
        newBall.label.color = Color.black;

        newBall.collisionRecords.Clear();
        if (collisionRecords != null)
            foreach (var rec in collisionRecords) newBall.collisionRecords.Add(rec);
        newBall.triggeredIndex = 0;

        balls.Add(newBall);

        SetInfoText($"发射路径小球 #{newBall.ballID} → 目标 {targetIndex}，当前小球总数 {balls.Count}");
        Debug.Log($"LaunchBallWithPath: 小球 #{newBall.ballID}，目标 {targetIndex}，路径点 {path.Count}");
    }

    private void CheckBatchComplete()
    {
        if (!IsRunning) return;
        bool hasIncomplete = false;
        foreach (var ball in balls)
        {
            if (ball.batchId == currentBatchId && !ball.isComplete)
            {
                hasIncomplete = true;
                break;
            }
        }
        if (!hasIncomplete && !batchCompletedNotified)
        {
            batchCompletedNotified = true;
            IsRunning = false;
            onAllCompleteCallback?.Invoke();
            onAllCompleteCallback = null;
            Debug.Log($"批次 {currentBatchId} 所有小球完成");
        }
    }

    private void UpdateRunStatus()
    {
        if (!IsRunning)
        {
            if (runStatusText != null) { runStatusText.text = "就绪"; runStatusText.color = Color.white; }
            return;
        }
        int completed = 0, total = 0;
        foreach (var ball in balls)
        {
            if (ball.batchId == currentBatchId)
            {
                total++;
                if (ball.isComplete) completed++;
            }
        }
        if (total == 0 && !batchCompletedNotified)
        {
            if (runStatusText != null) { runStatusText.text = "运行中: 完成"; runStatusText.color = Color.green; }
            return;
        }
        if (runStatusText != null) { runStatusText.text = $"运行中: {completed}/{total}"; runStatusText.color = Color.yellow; }
    }
}