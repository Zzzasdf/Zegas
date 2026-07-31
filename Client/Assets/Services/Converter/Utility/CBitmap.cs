using UnityEngine;

/// 通用位图
/// 256 个 long
/// 支持 16384（64 * 256）个数据
/// 内存占用约 2KB
public static class CBitmap
{
    private const int Size = sizeof(long) * 8;
    private const int LimitCount = 256; 
    private static BitmapConverter bitmapConverter;

    static CBitmap()
    {
        bitmapConverter = new BitmapConverter();
    }

    public static void Clear()
    {
        bitmapConverter?.Clear();
    }

    public static bool HasFlag(uint flag)
    {
        if (!TryReadWrite(flag)) return false;
        return bitmapConverter.HasFlag(flag);
    }

    public static void SetFlag(uint flag, bool value)
    {
        if (!TryReadWrite(flag)) return;
        bitmapConverter.SetFlag(flag, value);
    }

    public static void AddFlag(uint flag)
    {
        if (!TryReadWrite(flag)) return;
        bitmapConverter.AddFlag(flag);
    }

    public static void RemoveFlag(uint flag)
    {
        if (!TryReadWrite(flag)) return;
        bitmapConverter.RemoveFlag(flag);
    }

    private static bool TryReadWrite(uint flag)
    {
        if (flag >= Size * LimitCount)
        {
            Debug.LogError($"超出读写范围 0-{Size * LimitCount - 1}, 当前 flag => {flag}");
            return false;
        }
        return true;
    }
}
