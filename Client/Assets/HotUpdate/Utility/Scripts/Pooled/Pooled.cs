using System;

public interface IReusable
{
    void Release();
}

public sealed class Pooled<T> : IDisposable
    where T : class, IReusable, new()
{
    private T _value;
    public T Value => _value;

    private static readonly MonitoredObjectPool.ObjectPool<Pooled<T>, T> s_Pool = 
        new("Pooled", () => new Pooled<T>(), 
            null,
            l => l._value.Release());

    public static UnityEngine.Pool.PooledObject<Pooled<T>> Get(out Pooled<T> value) => s_Pool.Get(out value);
    public static Pooled<T> Get() => s_Pool.Get();
    private Pooled() => _value = new T();
    void IDisposable.Dispose() => s_Pool.Release(this);

#if POOLED_EXCEPTION
    ~Pooled() => s_Pool.FinalizeDebug();
#endif
    
    public static implicit operator T(Pooled<T> self) => self._value;
}