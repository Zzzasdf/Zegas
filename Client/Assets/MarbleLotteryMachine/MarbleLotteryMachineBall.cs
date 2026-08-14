using UnityEngine;
using UnityEngine.UI;          // 用于 Image
using System.Collections;

/// <summary>
/// 弹珠抽奖机 -- 小球组件（支持 UI Image 和 3D Renderer）
/// </summary>
public class MarbleLotteryMachineBall : MonoBehaviour
{
    [SerializeField] private RectTransform collisionArea;

    public RectTransform CollisionArea => collisionArea;

    [Header("颜色反馈参数")]
    [SerializeField] private float hitFlashDuration = 0.15f;
    [SerializeField] private float reachFlashInterval = 0.1f;
    [SerializeField] private int reachFlashCount = 4;
    [SerializeField] private Color reachColor = Color.green;   // 到达目标区域的颜色

    // 缓存组件
    private Image image;
    private Renderer renderer;
    private Color originalColor;
    private bool isUI;      // 标记是否为 UI

    private void Awake()
    {
        // 尝试获取 Image 或 Renderer
        image = GetComponent<Image>();
        renderer = GetComponent<Renderer>();
        isUI = image != null;

        if (!isUI && renderer == null)
        {
            Debug.LogWarning($"小球 {gameObject.name} 缺少 Image 或 Renderer 组件，颜色反馈无效。");
            return;
        }

        // 保存初始颜色
        originalColor = isUI ? image.color : renderer.material.color;
    }

    /// <summary>
    /// 撞击障碍物 —— 红色闪烁
    /// </summary>
    public void HitObstacle()
    {
        Debug.Log($"{gameObject.name} 执行 HitObstacle()");
        StopCoroutine(nameof(HitEffectCoroutine));
        StartCoroutine(HitEffectCoroutine());
    }

    /// <summary>
    /// 到达目标区域 —— 指定颜色闪烁庆祝
    /// </summary>
    public void ReachFinalArea()
    {
        Debug.Log($"{gameObject.name} 执行 ReachFinalArea()");
        StopCoroutine(nameof(ReachEffectCoroutine));
        StartCoroutine(ReachEffectCoroutine());
    }

    // ---------- 颜色设置工具 ----------
    private void SetColor(Color color)
    {
        if (isUI)
            image.color = color;
        else
            renderer.material.color = color;
    }

    // ---------- 协程 ----------
    private IEnumerator HitEffectCoroutine()
    {
        if (!HasValidComponent()) yield break;

        SetColor(Color.red);
        yield return new WaitForSeconds(hitFlashDuration);
        SetColor(originalColor);
    }

    private IEnumerator ReachEffectCoroutine()
    {
        if (!HasValidComponent()) yield break;

        for (int i = 0; i < reachFlashCount; i++)
        {
            SetColor((i % 2 == 0) ? reachColor : originalColor);
            yield return new WaitForSeconds(reachFlashInterval);
        }

        // 最终停留在目标颜色
        SetColor(reachColor);
    }

    private bool HasValidComponent()
    {
        if (isUI || renderer != null) return true;
        Debug.LogWarning($"{gameObject.name} 没有可用的颜色组件，跳过效果。");
        return false;
    }
}