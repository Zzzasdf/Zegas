using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
        using (var pList = Pooled<List<int>, int>.Get())
        {
            pList.Value.Add(2);
            Debug.Assert(pList.Value.Count == 1);
        }
        using (var pList = Pooled<List<int>, int>.Get())
        {
            Debug.Assert(pList.Value.Count == 0);
        }
        
        using (var pList = Pooled<List<int>, int>.Get())
        {
            pList.Value.Add(2);
            Debug.Assert(pList.Value.Count == 1);
        }
    }

    [Test]
    public void PooledCharsFoo()
    {
        using (PooledChars pooled = PooledChars.Get())
        {
            pooled.Add("ABCD");
            Debug.Log(pooled);
            ReadOnlyMemory<char> a = pooled;
            MemoryMarshal.TryGetArray(a, out ArraySegment<char> segment);
            char[] b = segment.Array;
            Debug.Log(new string(b));
            b[0] = 'B';
            Debug.Log(pooled);
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
            using (Pooled<List<int>, int> pooledList = Pooled<List<int>, int>.Get())
            {
                pooledList.Value.Add(1);
            }
        }
        stopwatch.Stop();
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

    [Test]
    public void PooledCollectionFoo()
    {
        using (Pooled<List<int>, int> pooled = Pooled<List<int>, int>.Get())
        {
            List<int> list =  pooled;
            list.Add(1);
            for (int i = 0; i < list.Count; i++)
            {
                Debug.Log(list[i]);
            }
        }
    }

    [Test]
    public void PooledMapFoo()
    {
        using (Pooled<Dictionary<int, string>, int, string> pooled = Pooled<Dictionary<int, string>, int, string>.Get())
        {
            pooled.Value.Add(1, "A");
            foreach (var pair in pooled.Value)
            {
                Debug.Log(pair.Key + ": "  + pair.Value);
            }
        }
        using (Pooled<Dictionary<string, long>, string, long> pooled = Pooled<Dictionary<string, long>, string, long>.Get())
        {
            pooled.Value.Add("A", 1);
            foreach (var pair in pooled.Value)
            {
                Debug.Log(pair.Key + ": "  + pair.Value);
            }
        }
    }
}

