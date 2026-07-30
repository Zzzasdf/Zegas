using System;

public sealed class PooledCharArray : IDisposable
{
    private static readonly MonitoredObjectPool.ObjectPool<PooledCharArray, PooledCharArray> s_Pool = 
        new(nameof(PooledCharArray), () => new PooledCharArray(), 
            null,
            l => l.Clear());

    public static UnityEngine.Pool.PooledObject<PooledCharArray> Get(out PooledCharArray value) => s_Pool.Get(out value);
    public static PooledCharArray Get() => s_Pool.Get();
    
    private PooledCharArray() => _items = new char[4];
    void IDisposable.Dispose() => s_Pool.Release(this);

#if POOLED_EXCEPTION
    ~PooledCharArray() => s_Pool.FinalizeDebug();
#endif
    
    private char[] _items;
    private int _size;

    public static implicit operator ReadOnlyMemory<char>(PooledCharArray self)
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
    public PooledCharArray Add(long number, int digitPadLeft = 0)
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
    public PooledCharArray Add(char c)
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
    public PooledCharArray Add(string chars)
    {
        return AddRange(chars.AsSpan());
    }
    public PooledCharArray AddRange(ReadOnlySpan<char> spanC)
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
