using System;
using TMPro;

public class TimeFormat
{
    /// 显示格式
    public enum ETimeFormat
    {
        /// 电子格式 00:00:00
        ELEC_HourMinuteSecond = 10001,
        
        /// 中文格式 00时00分00秒
        CN_HourMinuteSecond = 20001,
    }

    public static void SetTMPTimeFormat(in TMP_Text tmpText, ETimeFormat eTimeFormat, long ticks)
    {
        using (PooledCharArray pooledCharArray = PooledCharArray.Get())
        {
            SetTimeFormat(pooledCharArray, eTimeFormat, ticks);
            pooledCharArray.SetText(tmpText);
        }
    }

    public static string GetTimeFormat(ETimeFormat eTimeFormat, long ticks)
    {
        using (PooledCharArray pooledCharArray = PooledCharArray.Get())
        {
            SetTimeFormat(pooledCharArray, eTimeFormat, ticks);
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
            case ETimeFormat.ELEC_HourMinuteSecond: // 电子格式 00:00:00
            {
                pooledCharArray.Add((int)span.TotalHours, 2).Add(':').Add(span.Minutes, 2).Add(':').Add(span.Seconds, 2);
                break;
            }
            case ETimeFormat.CN_HourMinuteSecond: // 中文格式 00时00分00秒
            {
                pooledCharArray.Add((int)span.TotalHours, 2).Add('时').Add(span.Minutes, 2).Add('分').Add(span.Seconds, 2).Add('秒');
                break;
            }
        }
    }
}
