using System.Collections.Generic;
using UnityEngine;

namespace Converter.List.Long
{
    /// 键值对 key => long, value => bit (64bit)
    public class MapLongBitConverter<TList> : IConverter<TList>
        where TList : IList<long>, new()
    {
        private const int Size = sizeof(long) * 8;
        private Dictionary<long, long> map;
        private TList source;

        void IConverter<TList>.Decode(TList source)
        {
            map?.Clear();
            if (source == null || source.Count < 2)
            {
                return;
            }
            map ??= new Dictionary<long, long>();
            for (int i = 0; i + 1 < source.Count; i += 2)
            {
                map.Add(source[i], source[i + 1]);
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
                this.source.Add(pair.Key);
                this.source.Add(pair.Value);
            }
            source = this.source;
            return true;
        }
        
        public bool this[long key, uint flag]
        {
            get => HasFlag(key, flag);
            set => SetFlag(key, flag, value);
        }
        
        public void Clear()
        {
            map?.Clear();
        }

        public bool ContainsKey(long key)
        {
            return map?.ContainsKey(key) == true;
        }

        public bool HasFlag(long key, uint flag)
        {
            if (!TryReadWrite(flag)) return false;
            if (map == null || !map.TryGetValue(key, out long value))
            {
                return false;
            }
            return (value & (1L << (int)flag)) != 0;
        }
        
        public void SetFlag(long key, uint flag, bool value)
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
        
        public void AddFlag(long key, uint flag)
        {
            if (!TryReadWrite(flag)) return;
            map ??= new Dictionary<long, long>();
            long value = 1L << (int)flag;
            if (!map.TryAdd(key, value))
            {
                map[key] |= (uint)value;
            }
        }

        public void RemoveFlag(long key, uint flag)
        {
            if (!TryReadWrite(flag)) return;
            if (map == null || !map.ContainsKey(key))
            {
                return;
            }
            long value = 1L << (int)flag;
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
    }
}