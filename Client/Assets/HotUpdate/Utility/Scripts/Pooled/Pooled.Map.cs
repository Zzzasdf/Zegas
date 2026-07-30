using System;
using System.Collections.Generic;

public sealed class Pooled<TMap, TKey, TValue> : IDisposable
    where TMap : class, ICollection<KeyValuePair<TKey, TValue>>, new()
{
    private TMap _value;
    public TMap Value => _value;

    private static readonly MonitoredObjectPool.ObjectPool<Pooled<TMap, TKey, TValue>, TMap> s_Pool = 
        new("PooledMap", () => new Pooled<TMap, TKey, TValue>(), 
            null,
            l => l._value.Clear());

    public static UnityEngine.Pool.PooledObject<Pooled<TMap, TKey, TValue>> Get(out Pooled<TMap, TKey, TValue> value) => s_Pool.Get(out value);
    public static Pooled<TMap, TKey, TValue> Get() => s_Pool.Get();
    private Pooled() => _value = new TMap();
    void IDisposable.Dispose() => s_Pool.Release(this);

#if POOLED_EXCEPTION
    ~Pooled() => s_Pool.FinalizeDebug();
#endif

    public static implicit operator TMap(Pooled<TMap, TKey, TValue> self) => self._value;
}