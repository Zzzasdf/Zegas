using System;
using System.Text;

public sealed class PooledStringBuilder: IDisposable
{
    private StringBuilder _value;
    public StringBuilder Value => _value;

    private static readonly MonitoredObjectPool.ObjectPool<PooledStringBuilder, StringBuilder> s_Pool = 
        new(nameof(PooledStringBuilder), () => new PooledStringBuilder(), 
            null,
            l => l._value.Clear());

    public static UnityEngine.Pool.PooledObject<PooledStringBuilder> Get(out PooledStringBuilder value) => s_Pool.Get(out value);
    public static PooledStringBuilder Get() => s_Pool.Get();
    private PooledStringBuilder() => _value = new StringBuilder();
    void IDisposable.Dispose() => s_Pool.Release(this);

#if POOLED_EXCEPTION
    ~PooledStringBuilder() => s_Pool.FinalizeDebug();
#endif
    
    public static implicit operator StringBuilder(PooledStringBuilder self) => self._value;
}
