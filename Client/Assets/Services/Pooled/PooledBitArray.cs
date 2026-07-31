// using System;
// using System.Collections;
//
// public sealed class PooledBitArray: IDisposable
// {
//     private BitArray _value;
//     public BitArray Value => _value;
//
//     private static readonly MonitoredObjectPool.ObjectPool<PooledBitArray, BitArray> s_Pool = 
//         new(nameof(PooledBitArray), () => new PooledBitArray(), 
//             null,
//             l => l._value.Length = 0);
//
//     public static UnityEngine.Pool.PooledObject<PooledBitArray> Get(out PooledBitArray value) => s_Pool.Get(out value);
//     public static PooledBitArray Get(int length) => Get(length, false);
//     public static PooledBitArray Get(int length, bool defaultValue)
//     {
//         PooledBitArray pooled = s_Pool.Get();
//         pooled._value.Length = length;
//         pooled._value.SetAll(defaultValue);
//         return pooled;
//     }
//     public static PooledBitArray Get(byte[] bytes)
//     {
//     }
//     public static PooledBitArray Get(bool[] values)
//     {
//         
//     }
//     public static PooledBitArray Get(int[] values)
//     {
//         
//     }
//     public static PooledBitArray Get(BitArray bits)
//     {
//         
//     }
//     private PooledBitArray() => _value = new BitArray(0);
//     void IDisposable.Dispose() => s_Pool.Release(this);
//
// #if POOLED_EXCEPTION
//     ~PooledBitArray() => s_Pool.FinalizeDebug();
// #endif
//     
//     public static implicit operator BitArray(PooledBitArray self) => self._value;
// }