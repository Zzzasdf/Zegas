using System;
using TMPro;

public static class DateFormat_TMP_Text
{
    public static void SetText(this TMP_Text self, DateFormat.EFormat eFormat, long millSeconds, 
        string? prefixStr, string? suffixStr)
    {
        using (PooledChars pooledChars = PooledChars.Get())
        {
            DateFormat.Set(pooledChars, eFormat, millSeconds, prefixStr, suffixStr);
            self.SetText(pooledChars);
        }
    }
    public static void SetText(this TMP_Text self, DateFormat.EFormat eFormat, long millSeconds, 
        ReadOnlySpan<char> prefixChars, ReadOnlySpan<char> suffixChars)
    {
        using (PooledChars pooledChars = PooledChars.Get())
        {
            DateFormat.Set(pooledChars, eFormat, millSeconds, prefixChars, suffixChars);
            self.SetText(pooledChars);
        }
    }
    public static void SetText(this TMP_Text self, DateFormat.EFormat eFormat, long millSeconds)
    {
        using (PooledChars pooledChars = PooledChars.Get())
        {
            DateFormat.Set(pooledChars, eFormat, millSeconds);
            self.SetText(pooledChars);
        }
    }
}
