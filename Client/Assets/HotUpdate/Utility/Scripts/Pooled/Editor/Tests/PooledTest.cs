using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Debug = UnityEngine.Debug;

[TestFixture]
public class PooledTest
{
    public class ExList<T> : List<T>, IReusable
    {
        void IReusable.Release()
        {
            Clear();
        }
    }
    
    [Test]
    public void PooledFoo()
    {
        using (var pExlist = Pooled<ExList<int>>.Get())
        {
            ExList<int> exList = pExlist;
            exList.Add(1);
            Debug.Assert(exList.Count == 1);
        }
        using (var pExlist = Pooled<ExList<int>>.Get())
        {
            ExList<int> exList = pExlist;
            Debug.Assert(exList.Count == 0);            
        }
    }

    [Test]
    public void PooledListFoo()
    {
        using (var pList = PooledList<int>.Get())
        {
            pList.Add(2);
            Debug.Assert(pList.Count == 1);
        }
        using (var pList = PooledList<int>.Get())
        {
            Debug.Assert(pList.Count == 0);
        }
        
        using (var pList = PooledList<long>.Get())
        {
            pList.Add(2);
            Debug.Assert(pList.Count == 1);
        }
    }

    [Test]
    public void PooledBitArrayFoo()
    {
        // ArrayPool<int>.Shared.Rent(5);
        // ArrayPool<int>.Shared.Return();
        // Span<T>
        var l = MemoryPool<int>.Shared;
            var d = l.Rent(5);
            ReadOnlySpan<char> chars = null;
            for (int i = 0; i < chars.Length; i++)
            {
                var char1 = chars[i];
            }
    }

    private const int loopTimer = 1_000_000;
    [Test]
    public void PooledPerformanceCompareFoo()
    {
        PooledPerformanceFoo();
        InstancePerformanceFoo();
    }
    [Test]
    public void PooledPerformanceFoo()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < loopTimer; i++)
        {
            using (PooledList<int> pooledList = PooledList<int>.Get())
            {
                pooledList.Add(1);
            }
        }

        stopwatch.Stop();
#if POOLED_EXCEPTION
        MonitoredObjectPool.Pools["PooledList"][typeof(int)].Clear();
#endif
        Debug.Log($"对象池循环 {loopTimer} 次的耗时：{stopwatch.ElapsedMilliseconds}毫秒");
    }
    [Test]
    public void InstancePerformanceFoo()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < loopTimer; i++)
        {
            List<int> pooledList = new List<int>();
            pooledList.Add(1);
        }
        stopwatch.Stop();
        Debug.Log($"实例化循环 {loopTimer} 次的耗时：{stopwatch.ElapsedMilliseconds}毫秒");
    }
}

