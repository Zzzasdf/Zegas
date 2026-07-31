using System;
using System.Collections.Generic;

public sealed class Pooled<TCollection, TItem> : IDisposable
    where TCollection : class, ICollection<TItem>, new()
{
    private TCollection _value;
    public TCollection Value => _value;

    private static readonly MonitoredObjectPool.ObjectPool<Pooled<TCollection, TItem>, TCollection> s_Pool = 
        new("PooledCollection", () => new Pooled<TCollection, TItem>(), 
            null,
            l => l._value.Clear());

    public static UnityEngine.Pool.PooledObject<Pooled<TCollection, TItem>> Get(out Pooled<TCollection, TItem> value) => s_Pool.Get(out value);
    public static Pooled<TCollection, TItem> Get() => s_Pool.Get();
    private Pooled() => _value = new TCollection();
    void IDisposable.Dispose() => s_Pool.Release(this);

#if POOLED_EXCEPTION
    ~Pooled() => s_Pool.FinalizeDebug();
#endif
    
    public static implicit operator TCollection(Pooled<TCollection, TItem> self) => self._value;
}