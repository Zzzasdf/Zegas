using System;
using TMPro;

public static class TimeFormat_TMP_Text
{
    public static void SetText(this TMP_Text self, TimeFormat.EFormat eFormat, long millSeconds, 
        string? prefixStr, string? suffixStr)
    {
        using (PooledCharArray pooledCharArray = PooledCharArray.Get())
        {
            TimeFormat.Set(pooledCharArray, eFormat, millSeconds, prefixStr, suffixStr);
            self.SetText(pooledCharArray);
        }
    }
    public static void SetText(this TMP_Text self, TimeFormat.EFormat eFormat, long millSeconds, 
        ReadOnlySpan<char> prefixChars, ReadOnlySpan<char> suffixChars)
    {
        using (PooledCharArray pooledCharArray = PooledCharArray.Get())
        {
            TimeFormat.Set(pooledCharArray, eFormat, millSeconds, prefixChars, suffixChars);
            self.SetText(pooledCharArray);
        }
    }
    public static void SetText(this TMP_Text self, TimeFormat.EFormat eFormat, long millSeconds)
    {
        using (PooledCharArray pooledCharArray = PooledCharArray.Get())
        {
            TimeFormat.Set(pooledCharArray, eFormat, millSeconds);
            self.SetText(pooledCharArray);
        }
    }
}
