using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class DateUnit : MonoBehaviour
{
    [Test]
    public void DateUnitFoo()
    {
        long millSeconds = SystemTime.CurrentUnixTimeMilliseconds();
        
        Log(DateFormat.EDateFormat.ELEC_YearMonthDay_HourMinuteSecond, millSeconds);
        
        Log(DateFormat.EDateFormat.CN_YearMonthDay_HourMinuteSecond, millSeconds);
    }
    
    private void Log(DateFormat.EDateFormat eDateFormat, long millSeconds)
    {
        string timeFormat = DateFormat.GetDateFormat(eDateFormat, millSeconds);
        Debug.Log(timeFormat);
    }
}