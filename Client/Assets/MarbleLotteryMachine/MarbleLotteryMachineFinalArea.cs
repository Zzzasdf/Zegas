using UnityEngine;
using UnityEngine.UI;
using LuaFramework;
using TKLua;

[CSLStub.Out("Activity/Marble")]
public class MarbleLotteryMachineFinalArea : CSLuaBehaviour
{
    [SerializeField] [Bind("collisionArea")] private RectTransform collisionArea;

    [Header("到达效果参数")]
    private Color reachColor = new Color(0f, 1f, 0f, 1f);
    private float flashDuration = 0.3f;

    private Image targetImage = null;
    private Color originalColor = new Color(1f, 1f, 1f, 1f);

    private bool isPlaying = false;
    private float effectStartTime = 0f;
    private Color effectStartColor = new Color(1f, 1f, 1f, 1f);

    public RectTransform CollisionArea { get { return this.collisionArea; } }

    private void Start()
    {
        if (this.collisionArea != null)
        {
            this.targetImage = this.collisionArea.gameObject.GetComponent<Image>();
        }
        if (this.targetImage == null)
        {
            this.targetImage = this.gameObject.GetComponent<Image>();
        }
        if (this.targetImage != null)
        {
            this.originalColor = new Color(this.targetImage.color.r, this.targetImage.color.g, this.targetImage.color.b, this.targetImage.color.a);
        }
        else
        {
            GLog.Debug("MarbleLotteryMachineFinalArea::Start", this.gameObject.name + " 无 Image 组件");
        }
    }

    private void Update()
    {
        if (!this.isPlaying || this.targetImage == null) return;
        float elapsed = Time.time - this.effectStartTime;
        float t = Mathf.Clamp01(elapsed / this.flashDuration);
        float eased = 1f - Mathf.Pow(1f - t, 2f);
        this.targetImage.color = Color.Lerp(this.effectStartColor, this.originalColor, eased);
        if (t >= 1f)
        {
            this.targetImage.color = new Color(this.originalColor.r, this.originalColor.g, this.originalColor.b, this.originalColor.a);
            this.isPlaying = false;
        }
    }

    public void Reach()
    {
        GLog.Debug("MarbleLotteryMachineFinalArea::Reach", "✅ 目标区域 " + this.gameObject.name + " 到达！");
        this.PlayEffect();
    }

    private void PlayEffect()
    {
        if (this.targetImage == null) return;
        this.targetImage.color = new Color(this.reachColor.r, this.reachColor.g, this.reachColor.b, this.reachColor.a);
        this.effectStartColor = new Color(this.reachColor.r, this.reachColor.g, this.reachColor.b, this.reachColor.a);
        this.effectStartTime = Time.time;
        this.isPlaying = true;
    }
}