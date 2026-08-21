using UnityEngine;
using TKLua;

[CSLStub.Out("Activity/Marble")]
public class MarbleLotteryMachineLauncher : CSLuaBehaviour
{
    [SerializeField] [Bind("launchPos")] private RectTransform launchPos;

    public RectTransform LaunchPos
    {
        get { return this.launchPos; }
    }
}