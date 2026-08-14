using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MarbleLotteryMachineObstacle : MonoBehaviour
{
    [Header("碰撞区域")]
    [SerializeField] private RectTransform collisionArea;

    [Header("碰撞效果参数")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private float scalePulse = 1.2f;

    private Image targetImage;
    private Color originalColor;
    private Vector3 originalScale;

    /// <summary>碰撞检测区域（供外部访问）</summary>
    public RectTransform CollisionArea => collisionArea;

    /// <summary>设置碰撞区域（运行时调用）</summary>
    public void SetCollisionArea(RectTransform rect)
    {
        collisionArea = rect;
    }

    void Start()
    {
        // 尝试从 collisionArea 获取 Image，若没有则从自身获取
        if (collisionArea != null)
        {
            targetImage = collisionArea.GetComponent<Image>();
            if (targetImage == null)
                targetImage = GetComponent<Image>();
        }
        else
        {
            targetImage = GetComponent<Image>();
        }

        if (targetImage != null)
            originalColor = targetImage.color;
        else
            Debug.LogWarning($"{name} 无 Image 组件，无法播放颜色效果");

        // 记录原始缩放（用于脉冲）
        if (collisionArea != null)
            originalScale = collisionArea.localScale;
        else
            originalScale = transform.localScale;
    }

    /// <summary>被小球撞击时调用</summary>
    public void Hit()
    {
        Debug.Log($"✅ {gameObject.name} 被击中！");
        PlayEffect();
    }

    /// <summary>播放撞击效果（闪烁 + 脉冲）</summary>
    private void PlayEffect()
    {
        StopAllCoroutines();
        StartCoroutine(EffectCoroutine());
    }

    private IEnumerator EffectCoroutine()
    {
        // ---- 1. 颜色闪烁 ----
        if (targetImage != null)
        {
            targetImage.color = hitColor;
            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flashDuration;
                float easedT = 1f - Mathf.Pow(1f - t, 2f);
                targetImage.color = Color.Lerp(hitColor, originalColor, easedT);
                yield return null;
            }
            targetImage.color = originalColor;
        }

        // ---- 2. 缩放脉冲 ----
        Transform targetTransform = collisionArea != null ? collisionArea : transform;
        targetTransform.localScale = originalScale * scalePulse;
        yield return new WaitForSeconds(flashDuration * 0.5f);
        float elapsed2 = 0f;
        Vector3 startScale = targetTransform.localScale;
        while (elapsed2 < flashDuration * 0.5f)
        {
            elapsed2 += Time.deltaTime;
            float t = elapsed2 / (flashDuration * 0.5f);
            targetTransform.localScale = Vector3.Lerp(startScale, originalScale, t);
            yield return null;
        }
        targetTransform.localScale = originalScale;
    }
}