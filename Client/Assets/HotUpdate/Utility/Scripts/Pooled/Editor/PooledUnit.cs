using System;
using System.Buffers;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Pool;

[TestFixture]
public class PooledUnit
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
    public void PooledBitArray()
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
}

