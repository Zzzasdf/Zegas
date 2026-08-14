using UnityEngine;

/// <summary>
/// 弹珠抽奖机 -- 发射器组件
/// </summary>
public class MarbleLotteryMachineLauncher : MonoBehaviour
{
    [SerializeField] private RectTransform launchPos;
    
    /// 发射位置
    public RectTransform LaunchPos => launchPos;
}
