using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class DateUnit : MonoBehaviour
{
    [Test]
    public void Foo()
    {
        Debug.Log(SystemTime.CurrentUnixTimeMilliseconds());
        string dateFormat = DateFormat.GetDateFormat(DateFormat.EDateFormat.ELEC_YearMonthDay_HourMinuteSecond, SystemTime.CurrentUnixTimeMilliseconds());
        Debug.Log(dateFormat);
    }
}