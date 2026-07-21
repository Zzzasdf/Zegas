using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class TimeUnit : MonoBehaviour
{
    [Test]
    public void TimeUnitFoo()
    {
        long millSeconds = SystemTime.CurrentUnixTimeMilliseconds();
        
        Log(TimeFormat.ETimeFormat.ELEC_DayHourMinuteSecond, millSeconds);
        Log(TimeFormat.ETimeFormat.ELEC_HourMinuteSecond, millSeconds);
        
        Log(TimeFormat.ETimeFormat.CN_DayHourMinuteSecond, millSeconds);
        Log(TimeFormat.ETimeFormat.CN_HourMinuteSecond, millSeconds);
    }

    private void Log(TimeFormat.ETimeFormat eTimeFormat, long millSeconds)
    {
        string timeFormat = TimeFormat.GetTimeFormat(eTimeFormat, millSeconds);
        Debug.Log(timeFormat);
    }
}
