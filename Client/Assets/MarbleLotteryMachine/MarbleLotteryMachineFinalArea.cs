using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 弹珠抽奖机 -- 目标区域组件（仅颜色反馈）
/// </summary>
public class MarbleLotteryMachineFinalArea : MonoBehaviour
{
    [SerializeField] private RectTransform collisionArea;

    [Header("到达效果参数")]
    [SerializeField] private Color reachColor = Color.green;   // 到达时的目标颜色
    [SerializeField] private float flashDuration = 0.3f;       // 颜色过渡时长

    private Image targetImage;
    private Color originalColor;

    public RectTransform CollisionArea => collisionArea;

    void Start()
    {
        // 优先从 collisionArea 获取 Image，否则从自身获取
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
    }

    /// <summary>
    /// 外部调用，表示小球到达目标区域
    /// </summary>
    public void Reach()
    {
        Debug.Log($"✅ 目标区域 {gameObject.name} 到达！");
        PlayEffect();
    }

    private void PlayEffect()
    {
        StopAllCoroutines();
        StartCoroutine(EffectCoroutine());
    }

    private IEnumerator EffectCoroutine()
    {
        if (targetImage == null) yield break;

        // 瞬间变为目标色，然后平滑过渡回原色
        targetImage.color = reachColor;
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            // 缓出效果，让颜色变化更自然
            float eased = 1f - Mathf.Pow(1f - t, 2f);
            targetImage.color = Color.Lerp(reachColor, originalColor, eased);
            yield return null;
        }
        targetImage.color = originalColor;
    }
}