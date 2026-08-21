using UnityEngine;
using UnityEngine.UI;
using TKLua;
using LuaFramework;

[CSLStub.Out("Activity/Marble")]
public class MarbleLotteryMachineBall : CSLuaBehaviour
{
    [SerializeField] [Bind("collisionArea")] private RectTransform collisionArea;
    public RectTransform CollisionArea
    {
        get { return this.collisionArea; }
    }

    [Header("颜色反馈参数")]
    private float hitFlashDuration = 0.15f;
    private float reachFlashInterval = 0.1f;
    private int reachFlashCount = 4;
    private Color reachColor = new Color(0f, 1f, 0f, 1f);

    private Image image = null;
    private Renderer renderer = null;
    private Color originalColor = new Color(1f, 1f, 1f, 1f);
    private bool isUI = false;

    private enum EffectType { None, Hit, Reach }
    private EffectType currentEffect = EffectType.None;
    private int flashIndex = 0;
    private float effectStartTime = 0f;
    private float nextFlashTime = 0f;

    private void Awake()
    {
        this.image = this.gameObject.GetComponent<Image>();
        this.renderer = this.gameObject.GetComponent<Renderer>();
        if (this.image != null)
        {
            this.isUI = true;
            this.originalColor = new Color(this.image.color.r, this.image.color.g, this.image.color.b, this.image.color.a);
        }
        else
        {
            this.isUI = false;
            if (this.renderer != null)
            {
                this.originalColor = new Color(this.renderer.material.color.r, this.renderer.material.color.g, this.renderer.material.color.b, this.renderer.material.color.a);
            }
            else
            {
                GLog.Debug("MarbleLotteryMachineBall::Awake", "小球 " + this.gameObject.name + " 缺少 Image 或 Renderer 组件，颜色反馈无效。");
                return;
            }
        }
    }

    private void Update()
    {
        if (this.currentEffect == EffectType.None) return;

        if (this.currentEffect == EffectType.Hit)
        {
            if (Time.time - this.effectStartTime >= this.hitFlashDuration)
            {
                this.SetColor(new Color(this.originalColor.r, this.originalColor.g, this.originalColor.b, this.originalColor.a));
                this.currentEffect = EffectType.None;
            }
        }
        else if (this.currentEffect == EffectType.Reach)
        {
            if (Time.time >= this.nextFlashTime)
            {
                if (this.flashIndex % 2 == 0)
                {
                    this.SetColor(new Color(this.reachColor.r, this.reachColor.g, this.reachColor.b, this.reachColor.a));
                }
                else
                {
                    this.SetColor(new Color(this.originalColor.r, this.originalColor.g, this.originalColor.b, this.originalColor.a));
                }
                this.flashIndex = this.flashIndex + 1;
                this.nextFlashTime = Time.time + this.reachFlashInterval;
                if (this.flashIndex >= this.reachFlashCount * 2)
                {
                    this.SetColor(new Color(this.reachColor.r, this.reachColor.g, this.reachColor.b, this.reachColor.a));
                    this.currentEffect = EffectType.None;
                }
            }
        }
    }

    private void SetColor(Color color)
    {
        if (this.isUI)
        {
            if (this.image != null)
            {
                this.image.color = new Color(color.r, color.g, color.b, color.a);
            }
        }
        else if (this.renderer != null)
        {
            this.renderer.material.color = new Color(color.r, color.g, color.b, color.a);
        }
    }

    public void HitObstacle()
    {
        GLog.Debug("MarbleLotteryMachineBall::HitObstacle", this.gameObject.name + " 执行 HitObstacle()");
        if (this.currentEffect == EffectType.Reach)
        {
            this.SetColor(new Color(this.originalColor.r, this.originalColor.g, this.originalColor.b, this.originalColor.a));
            this.currentEffect = EffectType.None;
        }
        if (this.currentEffect == EffectType.Hit)
        {
            this.effectStartTime = Time.time;
            return;
        }
        this.currentEffect = EffectType.Hit;
        this.effectStartTime = Time.time;
        this.SetColor(new Color(1f, 0f, 0f, 1f));
    }

    public void ReachFinalArea()
    {
        GLog.Debug("MarbleLotteryMachineBall::ReachFinalArea", this.gameObject.name + " 执行 ReachFinalArea()");
        this.currentEffect = EffectType.None;
        this.SetColor(new Color(this.originalColor.r, this.originalColor.g, this.originalColor.b, this.originalColor.a));
        this.currentEffect = EffectType.Reach;
        this.flashIndex = 0;
        this.nextFlashTime = Time.time;
        this.SetColor(new Color(this.reachColor.r, this.reachColor.g, this.reachColor.b, this.reachColor.a));
        this.flashIndex = this.flashIndex + 1;
        this.nextFlashTime = Time.time + this.reachFlashInterval;
    }
}