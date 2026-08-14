using UnityEngine;

/// <summary>
/// 弹珠抽奖机 -- 活动区域组件
/// </summary>
public class MarbleLotteryMachineActivityArea: MonoBehaviour
{
    [SerializeField] private RectTransform activityArea;

    /// 活动检测区域
    public RectTransform ActivityArea => activityArea;
}
