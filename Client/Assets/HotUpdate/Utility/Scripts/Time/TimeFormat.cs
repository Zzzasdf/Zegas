using System;
using TMPro;

public class TimeFormat
{
    /// 显示格式
    public enum ETimeFormat
    {
        /// 电子格式 00:00:00:00
        ELEC_DayHourMinuteSecond = 10001,
        /// 电子格式 00:00:00
        ELEC_HourMinuteSecond = 10002,
        
        /// 中文格式 0天0时0分0秒
        CN_DayHourMinuteSecond = 20001,
        /// 中文格式 0时0分0秒
        CN_HourMinuteSecond = 20002,
    }

    public static void SetTMPTimeFormat(in TMP_Text tmpText, ETimeFormat eTimeFormat, long millSeconds)
    {
        using (PooledCharArray pooledCharArray = PooledCharArray.Get())
        {
            SetTimeFormat(pooledCharArray, eTimeFormat, millSeconds);
            pooledCharArray.SetText(tmpText);
        }
    }

    public static string GetTimeFormat(ETimeFormat eTimeFormat, long millSeconds)
    {
        using (PooledCharArray pooledCharArray = PooledCharArray.Get())
        {
            SetTimeFormat(pooledCharArray, eTimeFormat, millSeconds);
            return pooledCharArray.ToString();
        }
    }
    
    /// 设置显示字符数组
    private static void SetTimeFormat(in PooledCharArray pooledCharArray, ETimeFormat eTimeFormat, long millSeconds)
    {
        if (millSeconds <= 0)
        {
            return;
        }
        TimeSpan span = TimeSpan.FromMilliseconds(millSeconds);
        switch (eTimeFormat)
        {
            case ETimeFormat.ELEC_DayHourMinuteSecond: // 电子格式 00:00:00:00
            {
                pooledCharArray.Add((int)span.TotalDays).Add('天')
                    .Add(span.Hours, 2).Add(':').Add(span.Minutes, 2).Add(':').Add(span.Seconds, 2);
                break;
            }
            case ETimeFormat.ELEC_HourMinuteSecond: // 电子格式 00:00:00
            {
                pooledCharArray.Add((int)span.TotalHours, 2).Add(':').Add(span.Minutes, 2).Add(':').Add(span.Seconds, 2);
                break;
            }
            
            case ETimeFormat.CN_DayHourMinuteSecond: // 中文格式 0天00时00分00秒
            {
                pooledCharArray.Add((int)span.TotalDays).Add('天')
                    .Add(span.Hours).Add('时').Add(span.Minutes).Add('分').Add(span.Seconds).Add('秒');
                break;
            }
            case ETimeFormat.CN_HourMinuteSecond: // 中文格式 00时00分00秒
            {
                pooledCharArray.Add((int)span.TotalHours).Add('时').Add(span.Minutes).Add('分').Add(span.Seconds).Add('秒');
                break;
            }
        }
    }
}
