using System;
using TMPro;

public class TimeFormat
{
    /// 显示格式
    public enum EFormat
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
    
#region TMP_Text
    public static void SetTMP(in TMP_Text tmpText, EFormat eFormat, long millSeconds, 
        string? prefixStr, string? suffixStr)
    {
        using (PooledCharArray pooledCharArray = PooledCharArray.Get())
        {
            Set(pooledCharArray, eFormat, millSeconds, prefixStr, suffixStr);
            pooledCharArray.SetText(tmpText);
        }
    }
    public static void SetTMP(in TMP_Text tmpText, EFormat eFormat, long millSeconds, 
        ReadOnlySpan<char> prefixChars, ReadOnlySpan<char> suffixChars)
    {
        using (PooledCharArray pooledCharArray = PooledCharArray.Get())
        {
            Set(pooledCharArray, eFormat, millSeconds, prefixChars, suffixChars);
            pooledCharArray.SetText(tmpText);
        }
    }
    public static void SetTMP(in TMP_Text tmpText, EFormat eFormat, long millSeconds)
    {
        using (PooledCharArray pooledCharArray = PooledCharArray.Get())
        {
            Set(pooledCharArray, eFormat, millSeconds);
            pooledCharArray.SetText(tmpText);
        }
    }
#endregion

#region Get
    public static string Get(EFormat eFormat, long millSeconds, 
        string? prefixStr, string? suffixStr)
    {
        using (PooledCharArray pooledCharArray = PooledCharArray.Get())
        {
            Set(pooledCharArray, eFormat, millSeconds, prefixStr, suffixStr);
            return pooledCharArray.ToString();
        }
    }
    public static string Get(EFormat eFormat, long millSeconds, 
        ReadOnlySpan<char> prefixChars, ReadOnlySpan<char> suffixChars)
    {
        using (PooledCharArray pooledCharArray = PooledCharArray.Get())
        {
            Set(pooledCharArray, eFormat, millSeconds, prefixChars, suffixChars);
            return pooledCharArray.ToString();
        }
    }
    public static string Get(EFormat eFormat, long millSeconds)
    {
        using (PooledCharArray pooledCharArray = PooledCharArray.Get())
        {
            Set(pooledCharArray, eFormat, millSeconds);
            return pooledCharArray.ToString();
        }
    }
#endregion

#region Set
    public static void Set(in PooledCharArray pooledCharArray, EFormat eFormat, long millSeconds,
        string? prefixStr, string? suffixStr)
    {
        ReadOnlySpan<char> prefixChars = prefixStr.AsSpan();
        ReadOnlySpan<char> suffixChars = suffixStr.AsSpan();
        Set(pooledCharArray, eFormat, millSeconds, prefixChars, suffixChars);
    }
    public static void Set(in PooledCharArray pooledCharArray, EFormat eFormat, long millSeconds, 
        ReadOnlySpan<char> prefixChars, ReadOnlySpan<char> suffixChars)
    {
        pooledCharArray.AddRange(prefixChars);
        Set(pooledCharArray, eFormat, millSeconds);
        pooledCharArray.AddRange(suffixChars);
    }
    public static void Set(in PooledCharArray pooledCharArray, EFormat eFormat, long millSeconds)
    {
        if (millSeconds <= 0)
        {
            return;
        }
        TimeSpan span = TimeSpan.FromMilliseconds(millSeconds);
        switch (eFormat)
        {
            case EFormat.ELEC_DayHourMinuteSecond: // 电子格式 00:00:00:00
            {
                pooledCharArray.Add((int)span.TotalDays).Add('天')
                    .Add(span.Hours, 2).Add(':').Add(span.Minutes, 2).Add(':').Add(span.Seconds, 2);
                break;
            }
            case EFormat.ELEC_HourMinuteSecond: // 电子格式 00:00:00
            {
                pooledCharArray.Add((int)span.TotalHours, 2).Add(':').Add(span.Minutes, 2).Add(':').Add(span.Seconds, 2);
                break;
            }
                
            case EFormat.CN_DayHourMinuteSecond: // 中文格式 0天00时00分00秒
            {
                pooledCharArray.Add((int)span.TotalDays).Add('天')
                    .Add(span.Hours).Add('时').Add(span.Minutes).Add('分').Add(span.Seconds).Add('秒');
                break;
            }
            case EFormat.CN_HourMinuteSecond: // 中文格式 00时00分00秒
            {
                pooledCharArray.Add((int)span.TotalHours).Add('时').Add(span.Minutes).Add('分').Add(span.Seconds).Add('秒');
                break;
            }
        }
    }
#endregion
}
