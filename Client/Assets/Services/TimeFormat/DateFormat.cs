using System;

public static class DateFormat
{
     /// 显示格式
    public enum EFormat
    {
        /// 电子格式 0000-00-00 00:00:00
        ELEC_YearMonthDay_HourMinuteSecond = 10001,
        
        /// 中文格式 0年0月0日 0时0分0秒
        CN_YearMonthDay_HourMinuteSecond = 20001,
    }
     
#region Get
    public static string Get(EFormat eFormat, long millSeconds, 
        string? prefixStr, string? suffixStr)
    {
        using (PooledChars pooledChars = PooledChars.Get())
        {
            Set(pooledChars, eFormat, millSeconds, prefixStr, suffixStr);
            return pooledChars.ToString();
        }
    }
    public static string Get(EFormat eFormat, long millSeconds, 
        ReadOnlySpan<char> prefixChars, ReadOnlySpan<char> suffixChars)
    {
        using (PooledChars pooledChars = PooledChars.Get())
        {
            Set(pooledChars, eFormat, millSeconds, prefixChars, suffixChars);
            return pooledChars.ToString();
        }
    }
    public static string Get(EFormat eFormat, long millSeconds)
    {
        using (PooledChars pooledChars = PooledChars.Get())
        {
            Set(pooledChars, eFormat, millSeconds);
            return pooledChars.ToString();
        }
    }
#endregion
    
#region Set
    public static void Set(in PooledChars pooledChars, EFormat eFormat, long millSeconds,
        string? prefixStr, string? suffixStr)
    {
        ReadOnlySpan<char> prefixChars = prefixStr.AsSpan();
        ReadOnlySpan<char> suffixChars = suffixStr.AsSpan();
        Set(pooledChars, eFormat, millSeconds, prefixChars, suffixChars);
    }
    public static void Set(in PooledChars pooledChars, EFormat eFormat, long millSeconds,
        ReadOnlySpan<char> prefixChars, ReadOnlySpan<char> suffixChars)
    {
        pooledChars.AddRange(prefixChars);
        Set(pooledChars, eFormat, millSeconds);
        pooledChars.AddRange(suffixChars);
    }
    public static void Set(in PooledChars pooledChars, EFormat eFormat, long millSeconds)
    {
        DateTime date = SystemTime.GetDateTimeFromUnixMilliseconds(millSeconds);
        switch (eFormat)
        {
            case EFormat.ELEC_YearMonthDay_HourMinuteSecond: // 电子格式 0000-00-00 00:00:00
            {
                pooledChars.Add(date.Year, 4).Add('-').Add(date.Month, 2).Add('-').Add(date.Day, 2).Add(' ')
                    .Add(date.Hour, 2).Add(':').Add(date.Minute, 2).Add(':').Add(date.Second, 2);
                break;
            }

            case EFormat.CN_YearMonthDay_HourMinuteSecond: // 中文格式 0年0月0日 0时0分0秒
            {
                pooledChars.Add(date.Year).Add('年').Add(date.Month).Add('月').Add(date.Day).Add('日').Add(' ')
                    .Add(date.Hour).Add('时').Add(date.Minute).Add('分').Add(date.Second).Add('秒');
                break;
            }
        }
    }
#endregion
}
