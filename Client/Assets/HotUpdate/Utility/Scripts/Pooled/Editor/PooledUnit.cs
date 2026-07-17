using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class PooledUnit
{
    public class ExList<T> : List<T>, IReusable
    {
        void IReusable.Release()
        {
            Debug.Log("清空集合");
            Clear();
        }
    }
    
    
    [Test]
    public void Foo()
    {
        using (var exlist = Pooled<ExList<int>>.Get())
        {
            ExList<int> list = exlist;
            list.Add(1);
        }
        Debug.Log("Done");
    }
}

