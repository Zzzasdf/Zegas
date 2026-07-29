// using System;
// using NUnit.Framework;
// using UnityEngine;
//
// [TestFixture]
// public class TimeUnit : MonoBehaviour
// {
//     [Test]
//     public void TimeUnitFoo()
//     {
//         long millSeconds = SystemTime.CurrentUnixTimeMilliseconds();
//         
//         Log(TimeFormat.EFormat.ELEC_DayHourMinuteSecond, millSeconds, "前", "后");
//         Log(TimeFormat.EFormat.ELEC_HourMinuteSecond, millSeconds, "前", null);
//         
//         Log(TimeFormat.EFormat.CN_DayHourMinuteSecond, millSeconds, null, "后".AsSpan());
//         Log(TimeFormat.EFormat.CN_HourMinuteSecond, millSeconds);
//     }
//
//     private static void Log(TimeFormat.EFormat eFormat, long millSeconds,
//         string? prefixStr, string? suffixStr)
//     {
//         string timeFormat = TimeFormat.Get(eFormat, millSeconds, prefixStr, suffixStr);
//         Debug.Log(timeFormat);
//     }
//     private static void Log(TimeFormat.EFormat eFormat, long millSeconds, 
//         ReadOnlySpan<char> prefixChars, ReadOnlySpan<char> suffixChars)
//     {
//         string timeFormat = TimeFormat.Get(eFormat, millSeconds, prefixChars, suffixChars);
//         Debug.Log(timeFormat);
//     }
//     private void Log(TimeFormat.EFormat eFormat, long millSeconds)
//     {
//         string timeFormat = TimeFormat.Get(eFormat, millSeconds);
//         Debug.Log(timeFormat);
//     }
// }
