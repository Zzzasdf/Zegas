using System;

public static class SystemTime
{
    // Unix纪元（即1970年1月1日00:00:00 UTC）
    private static  readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly long unixTimeMilliseconds = 1678723200000L;
    
    // 东八区偏移量
    private static readonly double utc8MillisecondsOffset = 8 * 60  * 60 * 1000;

    /// 获取当前的Unix时间戳（毫秒级）
    public static long CurrentUnixTimeMilliseconds()
    {
        return DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
    
    /// 获取日期
    public static DateTime GetDateTimeFromUnixMilliseconds(long unixMilliseconds)
    {
        // 使用DateTimeOffset.FromUnixTimeMilliseconds方法将毫秒数转换为DateTimeOffset对象
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds);
        
        // 从DateTimeOffset对象中提取UTC时间的DateTime对象
        DateTime dateTimeUtc = dateTimeOffset.UtcDateTime.AddMilliseconds(utc8MillisecondsOffset);
        // 返回UTC时间的DateTime对象
        return dateTimeUtc;
    }
    
    /// 是否在同一天
    public static bool AreSameDay(long unixMilliseconds1, long unixMilliseconds2)
    {
        // 将毫秒数转换为DateTime对象（UTC时间）
        DateTime dateTime1 = DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds1).UtcDateTime;
        DateTime dateTime2 = DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds2).UtcDateTime;
 
        // 比较年、月和日部分
        return dateTime1.Year == dateTime2.Year &&
               dateTime1.Month == dateTime2.Month &&
               dateTime1.Day == dateTime2.Day;
    }
    
    /// 是否在今天
    public static bool IsToday(long unixMilliseconds)
    {
        // 将毫秒数转换为DateTime对象（本地时间）
        DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).LocalDateTime;
 
        // 获取今天的起始时间和结束时间（基于本地时区）
        DateTime todayStart = DateTime.Today; // 这将返回今天00:00:00的时间
        DateTime todayEnd = todayStart.AddDays(1); // 这将返回明天00:00:00的时间，因此今天的结束时间是今天的23:59:59.999...
 
        // 检查dateTime是否在todayStart和todayEnd之间
        return dateTime >= todayStart && dateTime < todayEnd;
    }
}
