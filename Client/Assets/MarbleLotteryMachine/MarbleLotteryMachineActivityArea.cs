using UnityEngine;
using TKLua;

[CSLStub.Out("Activity/Marble")]
public class MarbleLotteryMachineActivityArea : CSLuaBehaviour
{
    [SerializeField] [Bind("activityArea")] private RectTransform activityArea;

    public RectTransform ActivityArea
    {
        get { return this.activityArea; }
    }
}