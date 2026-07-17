using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class TimeUnit : MonoBehaviour
{
    [Test]
    public void Foo()
    {
        string timeFormat = TimeFormat.GetTimeFormat(TimeFormat.ETimeFormat.ELEC_HourMinuteSecond, (24 * 60 * 60 + 5 * 60 + 30) * 1000);
        Debug.Log(timeFormat);
    }
}
