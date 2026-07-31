using System.Collections.Generic;
using UnityEngine;

namespace Converter.List.Long
{
    /// 键值对 key => int, value => bit (32bit)
    public class MapIntBitConverter<TList> : IConverter<TList>
        where TList : IList<long>, new()
    {
        private const int Size = sizeof(int) * 8;
        private Dictionary<int, int> map;
        private TList source;

        void IConverter<TList>.Decode(TList source)
        {
            map?.Clear();
            if (source == null || source.Count == 0)
            {
                return;
            }
            map ??= new Dictionary<int, int>();
            foreach (long l in source)
            {
                (int high, int low) = LongConvertDoubleInt(l);
                map.Add(high, low);
            }
        }

        bool IConverter<TList>.TryEncode(out TList source)
        {
            if (map == null || map.Count == 0)
            {
                source = default;
                return false;
            }
            this.source ??= new TList();
            this.source.Clear();
            foreach (var pair in map)
            {
                this.source.Add(DoubleIntCombineLong(pair.Key, pair.Value));
            }
            source = this.source;
            return true;
        }

        public bool this[int key, uint flag]
        {
            get => HasFlag(key, flag);
            set => SetFlag(key, flag, value);
        }

        public void Clear()
        {
            map?.Clear();
        }

        public bool ContainsKey(int key)
        {
            return map?.ContainsKey(key) == true;
        }

        public bool HasFlag(int key, uint flag)
        {
            if (!TryReadWrite(flag)) return false;
            if (map == null || !map.TryGetValue(key, out int value))
            {
                return false;
            }
            return (value & (1 << (int)flag)) != 0;
        }

        public void SetFlag(int key, uint flag, bool value)
        {
            if (value)
            {
                AddFlag(key, flag);
            }
            else
            {
                RemoveFlag(key, flag);
            }
        }

        public void AddFlag(int key, uint flag)
        {
            if (!TryReadWrite(flag)) return;
            map ??= new Dictionary<int, int>();
            int value = 1 << (int)flag;
            if (!map.TryAdd(key, value))
            {
                map[key] |= value;
            }
        }

        public void RemoveFlag(int key, uint flag)
        {
            if (!TryReadWrite(flag)) return;
            if (map == null || !map.ContainsKey(key))
            {
                return;
            }
            int value = 1 << (int)flag;
            map[key] &= ~value;
            if (map[key] != 0)
            {
                return;
            }
            map.Remove(key);
        }

        private bool TryReadWrite(uint flag)
        {
            if (flag >= Size)
            {
                Debug.LogError($"超出读写范围 0-{Size - 1}, 当前 flag => {flag}");
                return false;
            }
            return true;
        }

        private (int high, int low) LongConvertDoubleInt(long l)
        {
            return ((int)(l >> 32), (int)(l & 0xFFFFFFFF));
        }

        private long DoubleIntCombineLong(int high, int low)
        {
            return ((long)high << 32) | (low & 0xFFFFFFFFL);
        }
    }
}