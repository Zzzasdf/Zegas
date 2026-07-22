using System;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class DateUnit : MonoBehaviour
{
    [Test]
    public void DateUnitFoo()
    {
        long millSeconds = SystemTime.CurrentUnixTimeMilliseconds();
        
        Log(DateFormat.EFormat.ELEC_YearMonthDay_HourMinuteSecond, millSeconds, "前", "后");
        Log(DateFormat.EFormat.ELEC_YearMonthDay_HourMinuteSecond, millSeconds, "前", null);
        
        Log(DateFormat.EFormat.CN_YearMonthDay_HourMinuteSecond, millSeconds, null, "后".AsSpan());
        Log(DateFormat.EFormat.CN_YearMonthDay_HourMinuteSecond, millSeconds);
    }
    
    private static void Log(DateFormat.EFormat eFormat, long millSeconds,
        string? prefixStr, string? suffixStr)
    {
        string timeFormat = DateFormat.Get(eFormat, millSeconds, prefixStr, suffixStr);
        Debug.Log(timeFormat);
    }
    private static void Log(DateFormat.EFormat eFormat, long millSeconds, 
        ReadOnlySpan<char> prefixChars, ReadOnlySpan<char> suffixChars)
    {
        string timeFormat = DateFormat.Get(eFormat, millSeconds, prefixChars, suffixChars);
        Debug.Log(timeFormat);
    }
    private void Log(DateFormat.EFormat eFormat, long millSeconds)
    {
        string timeFormat = DateFormat.Get(eFormat, millSeconds);
        Debug.Log(timeFormat);
    }
}