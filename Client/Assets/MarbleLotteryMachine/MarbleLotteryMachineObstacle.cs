using UnityEngine;
using UnityEngine.UI;
using LuaFramework;
using TKLua;

[CSLStub.Out("Activity/Marble")]
public class MarbleLotteryMachineObstacle : CSLuaBehaviour
{
    [Header("碰撞区域")]
    [SerializeField] [Bind("collisionArea")] private RectTransform collisionArea;

    [Header("碰撞效果参数")]
    private Color hitColor = new Color(1f, 0f, 0f, 1f);
    private float flashDuration = 0.2f;
    private float scalePulse = 1.2f;

    private Image targetImage = null;
    private Color originalColor = new Color(1f, 1f, 1f, 1f);
    private Vector3 originalScale = new Vector3(1f, 1f, 1f);

    private bool isPlayingEffect = false;
    private Color effectStartColor = new Color(1f, 1f, 1f, 1f);
    private Vector3 effectStartScale = new Vector3(1f, 1f, 1f);
    private Vector3 effectTargetScale = new Vector3(1f, 1f, 1f);

    private enum EffectPhase { Color, ScalePulse, ScaleReturn, Done }
    private EffectPhase currentPhase = EffectPhase.Done;
    private float phaseStartTime = 0f;

    public RectTransform CollisionArea
    {
        get { return this.collisionArea; }
    }
    public void SetCollisionArea(RectTransform rect) { this.collisionArea = rect; }

    private Vector3 Vector3Lerp(Vector3 a, Vector3 b, float t)
    {
        return new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
    }

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
            GLog.Debug("MarbleLotteryMachineObstacle::Start", this.gameObject.name + " 无 Image 组件");
        }

        Transform targetTransform = this.collisionArea;
        if (targetTransform != null)
        {
            this.originalScale = new Vector3(targetTransform.localScale.x, targetTransform.localScale.y, targetTransform.localScale.z);
        }
    }

    private void Update()
    {
        if (!this.isPlayingEffect) return;

        float elapsed = Time.time - this.phaseStartTime;
        Transform targetTransform = this.collisionArea;
        if (targetTransform == null) return;

        if (this.currentPhase == EffectPhase.Color)
        {
            float t = Mathf.Clamp01(elapsed / this.flashDuration);
            float eased = 1f - Mathf.Pow(1f - t, 2f);
            if (this.targetImage != null)
            {
                this.targetImage.color = Color.Lerp(this.effectStartColor, this.originalColor, eased);
            }
            if (t >= 1f)
            {
                if (this.targetImage != null)
                {
                    this.targetImage.color = new Color(this.originalColor.r, this.originalColor.g, this.originalColor.b, this.originalColor.a);
                }
                this.currentPhase = EffectPhase.ScalePulse;
                this.phaseStartTime = Time.time;
                this.effectStartScale = new Vector3(targetTransform.localScale.x, targetTransform.localScale.y, targetTransform.localScale.z);
                this.effectTargetScale = new Vector3(this.originalScale.x * this.scalePulse, this.originalScale.y * this.scalePulse, this.originalScale.z * this.scalePulse);
                targetTransform.localScale = new Vector3(this.effectTargetScale.x, this.effectTargetScale.y, this.effectTargetScale.z);
            }
        }
        else if (this.currentPhase == EffectPhase.ScalePulse)
        {
            if (elapsed >= this.flashDuration * 0.5f)
            {
                this.currentPhase = EffectPhase.ScaleReturn;
                this.phaseStartTime = Time.time;
                this.effectStartScale = new Vector3(targetTransform.localScale.x, targetTransform.localScale.y, targetTransform.localScale.z);
                this.effectTargetScale = new Vector3(this.originalScale.x, this.originalScale.y, this.originalScale.z);
            }
        }
        else if (this.currentPhase == EffectPhase.ScaleReturn)
        {
            float t = Mathf.Clamp01(elapsed / (this.flashDuration * 0.5f));
            Vector3 newScale = this.Vector3Lerp(this.effectStartScale, this.effectTargetScale, t);
            targetTransform.localScale = new Vector3(newScale.x, newScale.y, newScale.z);
            if (t >= 1f)
            {
                targetTransform.localScale = new Vector3(this.originalScale.x, this.originalScale.y, this.originalScale.z);
                this.isPlayingEffect = false;
            }
        }
        else
        {
            this.isPlayingEffect = false;
        }
    }

    public void Hit()
    {
        GLog.Debug("MarbleLotteryMachineObstacle::Hit", "✅ " + this.gameObject.name + " 被击中！");
        this.PlayEffect();
    }

    private void PlayEffect()
    {
        this.isPlayingEffect = false;
        if (this.targetImage != null)
        {
            this.targetImage.color = new Color(this.originalColor.r, this.originalColor.g, this.originalColor.b, this.originalColor.a);
        }

        Transform targetTransform = this.collisionArea;
        if (targetTransform == null) return;
        targetTransform.localScale = new Vector3(this.originalScale.x, this.originalScale.y, this.originalScale.z);

        if (this.targetImage != null)
        {
            this.targetImage.color = new Color(this.hitColor.r, this.hitColor.g, this.hitColor.b, this.hitColor.a);
            this.effectStartColor = new Color(this.hitColor.r, this.hitColor.g, this.hitColor.b, this.hitColor.a);
            this.currentPhase = EffectPhase.Color;
            this.phaseStartTime = Time.time;
            this.isPlayingEffect = true;
        }
        else
        {
            this.currentPhase = EffectPhase.ScalePulse;
            this.phaseStartTime = Time.time;
            this.effectStartScale = new Vector3(targetTransform.localScale.x, targetTransform.localScale.y, targetTransform.localScale.z);
            this.effectTargetScale = new Vector3(this.originalScale.x * this.scalePulse, this.originalScale.y * this.scalePulse, this.originalScale.z * this.scalePulse);
            targetTransform.localScale = new Vector3(this.effectTargetScale.x, this.effectTargetScale.y, this.effectTargetScale.z);
            this.isPlayingEffect = true;
        }
    }
}