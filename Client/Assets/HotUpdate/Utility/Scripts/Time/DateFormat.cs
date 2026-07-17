
using System;
using TMPro;

public class DateFormat
{
     /// 显示格式
    public enum EDateFormat
    {
        /// 电子格式 0000-00-00 00:00:00
        ELEC_YearMonthDay_HourMinuteSecond = 10001,
    }

    public static void SetTMPDateFormat(in TMP_Text tmpText, EDateFormat eDateFormat, long ticks)
    {
        using (PooledCharArray pooledCharArray = PooledCharArray.Get())
        {
            SetDateFormat(pooledCharArray, eDateFormat, ticks);
            pooledCharArray.SetText(tmpText);
        }
    }

    public static string GetDateFormat(EDateFormat eDateFormat, long ticks)
    {
        using (PooledCharArray pooledCharArray = PooledCharArray.Get())
        {
            SetDateFormat(pooledCharArray, eDateFormat, ticks);
            return pooledCharArray.ToString();
        }
    }
    
    /// 设置显示字符数组
    private static void SetDateFormat(in PooledCharArray pooledCharArray, EDateFormat EDateFormat, long millSeconds)
    {
        DateTime date = SystemTime.GetDateTimeFromUnixMilliseconds(millSeconds);
        switch (EDateFormat)
        {
            case EDateFormat.ELEC_YearMonthDay_HourMinuteSecond: // 电子格式 0000-00-00 00:00:00
            {
                pooledCharArray.Add(date.Year, 4).Add('-').Add(date.Month, 2).Add('-').Add(date.Day, 2).Add(' ')
                    .Add(date.Hour, 2).Add(':').Add(date.Minute, 2).Add(':').Add(date.Second, 2);
                break;
            }
        }
    }
}
