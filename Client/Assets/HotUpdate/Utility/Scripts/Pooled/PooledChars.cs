using System;

public sealed class PooledChars : IDisposable
{
    private char[] _items;
    private int _size;
    
    private static readonly MonitoredObjectPool.ObjectPool<PooledChars, PooledChars> s_Pool = 
        new(nameof(PooledChars), () => new PooledChars(), 
            null,
            l => l.Clear());

    public static UnityEngine.Pool.PooledObject<PooledChars> Get(out PooledChars value) => s_Pool.Get(out value);
    public static PooledChars Get() => s_Pool.Get();
    
    private PooledChars()
    {
        _items = new char[4];
        _size = 0;
    }

    void IDisposable.Dispose() => s_Pool.Release(this);

#if POOLED_EXCEPTION
    ~PooledChars() => s_Pool.FinalizeDebug();
#endif

    public static implicit operator ReadOnlyMemory<char>(PooledChars self)
    {
        return new ReadOnlyMemory<char>(self._items, 0, self._size);
    }
    
    private void Clear()
    {
        Array.Clear(_items, 0, _size);
        _size = 0;
    }
    
    /// <summary>
    /// 添加一组数值
    /// </summary>
    /// <param name="number"></param>
    /// <param name="digitPadLeft">不足位数补0</param>
    /// <returns></returns>
    public PooledChars Add(long number, int digitPadLeft = 0)
    {
        int startSize = _size;
        long digit;
        do
        {
            digit = number % 10;
            number /= 10;
            Add((char)('0' + digit));
        } while (number > 0 || _size - startSize < digitPadLeft);
        // 反转高低位
        for (int i = startSize, j = _size - 1, count = startSize + (_size - startSize) / 2; i < count; i++, j--)
        {
            (_items[i], _items[j]) = (_items[j], _items[i]);
        }
        return this;
    }
    public PooledChars Add(char c)
    {
        if (_size == _items.Length)
        {
            char[] newChars = new char[_items.Length * 2];
            Array.Copy(_items, newChars, _items.Length);
            _items = newChars;
        }
        _items[_size] = c;
        _size++;
        return this;
    }
    public PooledChars Add(string chars)
    {
        return AddRange(chars.AsSpan());
    }
    public PooledChars AddRange(ReadOnlySpan<char> spanC)
    {
        for (int i = 0; i < spanC.Length; i++)
        {
            Add(spanC[i]);
        }
        return this;
    }

    public override string ToString()
    {
        return new String(_items, 0, _size);
    }
}
