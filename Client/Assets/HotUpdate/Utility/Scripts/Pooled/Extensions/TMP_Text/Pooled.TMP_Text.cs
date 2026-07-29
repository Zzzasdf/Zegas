using System;
using System.Runtime.InteropServices;
using TMPro;

public static class Pooled_TMP_Text
{
    public static void SetText(this TMP_Text self, in PooledCharArray pooledCharArray)
    {
        ReadOnlyMemory<char> memory = pooledCharArray;
        if (!MemoryMarshal.TryGetArray(memory, out ArraySegment<char> segment))
        {
            return;
        }     
        self.SetText(segment.Array, segment.Offset, segment.Count);
    }
}