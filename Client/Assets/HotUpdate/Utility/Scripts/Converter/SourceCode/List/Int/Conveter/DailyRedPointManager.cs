using System;
using System.Collections.Generic;
using Converter.List.Int;
using UnityEngine;
using UnityEngine.Pool;

public enum EDailyRedPoint: uint
{
    /// 实际使用的 红点 需要小于 Max，尽量使用 32 的倍数
    Max = 32,
}

public static class EDailyRedPointExtensions
{
    // 订阅时会调用一次
    public static void SubscribeUpdate(this EDailyRedPoint eDailyRedPoint, Action<bool> func)
    {
        DailyRedPointManager.Instance.Subscribe(eDailyRedPoint, func);
    }
    public static void UnsubscribeUpdate(this EDailyRedPoint eDailyRedPoint, Action<bool> func)
    {
        DailyRedPointManager.Instance.Unsubscribe(eDailyRedPoint, func);
    }
    
    public static bool IsDisplay(this EDailyRedPoint eDailyRedPoint)
    {
        return DailyRedPointManager.Instance.IsDisplay(eDailyRedPoint);
    }
    public static void Remove(this EDailyRedPoint eDailyRedPoint)
    {
        DailyRedPointManager.Instance.Remove(eDailyRedPoint);
    }
}

public class DailyRedPointManager
{
    private static DailyRedPointManager _instance;
    public static DailyRedPointManager Instance => _instance ??= new DailyRedPointManager();
    private DailyRedPointManager() { }
    
    // int 的位数
    private const int Size = sizeof(int) * 8;
    // 缓存的 int 数量
    private const int Count = (int)EDailyRedPoint.Max / Size + ((int)EDailyRedPoint.Max % Size > 0 ? 1 : 0);
    // 上次重置的时间缓存 唯一标识
    private const string LastResetTimeUniqueTag = nameof(DailyRedPointManager);
    // 红点缓存 唯一标记
    private const string DataUniqueTag = nameof(EDailyRedPoint);
    
    private long rid;
    // 上次重置的时间
    private long lastResetTime;
    // 反转位图 默认为 true
    private BitmapReverseConverter<List<int>> bitmapConverter;
    
    // 事件响应器订阅集合
    private readonly Dictionary<EDailyRedPoint, Action<bool>> responseCollector = new Dictionary<EDailyRedPoint, Action<bool>>();
    private bool init;
    
    public bool TryInit(long rid)
    {
        if (rid == this.rid)
        {
            return false;
        }
        this.rid = rid;
        lastResetTime = GetLastResetTimeByMemory();
        bitmapConverter ??= new BitmapReverseConverter<List<int>>();
        bitmapConverter.Fill();
        if (!TryTimingReset())
        {
            // 不触发重置，则从内存中读取红点数据
            List<int> list = ListPool<int>.Get();
            for (int i = 0; i < Count; i++)
            {
                list.Add(GetData(i));
            }
            (bitmapConverter as  IConverter<List<int>>).Decode(list);
            ListPool<int>.Release(list);
        }
       
        // TODO ZZZ 需要监听跨天的接口变动，来重置跨天
        // ResetData()
        init = true;
        // 触发一次更新，避免在 Init 方法前被订阅
        FireAll();
        return true;
    }

    public void Clear()
    {
        init = false;
        responseCollector.Clear();
        // TODO ZZZ 移除监听
    }

    /// 重置数据
    private void ResetData()
    {
        if (TryTimingReset())
        {
            FireAll();
        }
    }
    
    /// 尝试重置，时机为两个时间不在同一天
    private bool TryTimingReset()
    {
        long currTime = GetCurrTime();
        bool isSameDay = IsSameDay(lastResetTime, currTime);
        if (!isSameDay)
        {
            // 保存一份当前重置的时间数据，并同步字段
            SetLastResetTimeByMemory(currTime);
            lastResetTime = currTime;
            // 将红点数据全部重置
            for (int i = 0; i < Count; i++)
            {
                SetData(i, 0);
            }
            bitmapConverter.Fill();
            return true;
        }
        return false;
    }

    private long GetLastResetTimeByMemory()
    {
        string keyPrefix = $"{LastResetTimeUniqueTag}_{rid}";
        int high = PlayerPrefs.GetInt($"{keyPrefix}_high", 0);
        int low = PlayerPrefs.GetInt($"{keyPrefix}_low", 0);
        return DoubleIntCombineLong(high, low);
    }
    private void SetLastResetTimeByMemory(long lastResetTime)
    {
        (int high, int low) = LongConvertDoubleInt(lastResetTime);
        string keyPrefix = $"{LastResetTimeUniqueTag}_{rid}";
        PlayerPrefs.SetInt($"{keyPrefix}_high", high);
        PlayerPrefs.SetInt($"{keyPrefix}_low", low);
    }
    private int GetData(int index) => PlayerPrefs.GetInt($"{DataUniqueTag}_{rid}_{index}", 0);
    private void SetData(int index, int value) => PlayerPrefs.SetInt($"{DataUniqueTag}_{rid}_{index}", value);
    private long GetCurrTime() => DateTimeOffset.Now.ToUnixTimeMilliseconds(); // TODO ZZZ

    public bool this[EDailyRedPoint eDailyRedPoint]
    {
        get => IsDisplay(eDailyRedPoint);
        set => Set(eDailyRedPoint, value);
    }      
    public bool IsDisplay(EDailyRedPoint eDailyRedPoint)
    {
        return !bitmapConverter.HasFlag((uint)eDailyRedPoint);
    }
    public void Set(EDailyRedPoint eDailyRedPoint, bool value)
    {
        bitmapConverter.SetFlag((uint)eDailyRedPoint, !value);
    }
    public void Add(EDailyRedPoint eDailyRedPoint)
    {
        if (IsDisplay(eDailyRedPoint)) return;
        bitmapConverter.AddFlag((uint)eDailyRedPoint);
        SaveListIndexDataByFlag(eDailyRedPoint);
        Fire(eDailyRedPoint, true);
    }
    public void Remove(EDailyRedPoint eDailyRedPoint)
    {
        if (!IsDisplay(eDailyRedPoint)) return;
        bitmapConverter.RemoveFlag((uint)eDailyRedPoint);
        SaveListIndexDataByFlag(eDailyRedPoint);
        Fire(eDailyRedPoint, false);
    }
    private void SaveListIndexDataByFlag(EDailyRedPoint eDailyRedPoint)
    {
        uint flag = (uint)eDailyRedPoint;
        (int index, int value)  = bitmapConverter.GetListIndexDataByFlag(flag);
        SetData(index, value);
    }
    
    /// 订阅更新响应
    public void Subscribe(EDailyRedPoint eDailyRedPoint, Action<bool> func)
    {
        if (func == null) return;
        if (!responseCollector.TryGetValue(eDailyRedPoint, out Action<bool> funcList))
        {
            funcList = func;
            responseCollector.Add(eDailyRedPoint, funcList);
        }
        else
        {
            responseCollector[eDailyRedPoint] += func;
        }
        func.Invoke(IsDisplay(eDailyRedPoint));
    }

    /// 移除订阅
    public void Unsubscribe(EDailyRedPoint eDailyRedPoint, Action<bool> func)
    {
        if (!responseCollector.ContainsKey(eDailyRedPoint))
        {
            return;
        }
        responseCollector[eDailyRedPoint] -= func;
    }

    /// 更新所有订阅
    private void FireAll()
    {
        if (!init) return;
        foreach (var pair in responseCollector)
        {
            EDailyRedPoint eDailyRedPoint = pair.Key;
            bool value = IsDisplay(eDailyRedPoint);
            Fire(eDailyRedPoint, value);
        }
    }
    
    /// 更新订阅
    private void Fire(EDailyRedPoint eDailyRedPoint, bool value)
    {
        if (!init) return;
        if (!responseCollector.TryGetValue(eDailyRedPoint, out Action<bool> funcList))
        {
            return;
        }
        funcList?.Invoke(value);
    }
    
    /// 判断两个时间是否为同一天
    private static bool IsSameDay(long timestamp1, long timestamp2) // 毫秒
    {
        // 将时间戳转换为 DateTime（本地时间）
        DateTime date1 = DateTimeOffset.FromUnixTimeMilliseconds(timestamp1).ToLocalTime().Date;
        DateTime date2 = DateTimeOffset.FromUnixTimeMilliseconds(timestamp2).ToLocalTime().Date;
        // 判断是否为同一天
        return date1 == date2;
    }
    
    private static (int high, int low) LongConvertDoubleInt(long l)
    {
        return ((int)(l >> Size), (int)(l & 0xFFFFFFFF));
    }

    private static long DoubleIntCombineLong(int high, int low)
    {
        return ((long)high << Size) | (low & 0xFFFFFFFFL);
    }
}