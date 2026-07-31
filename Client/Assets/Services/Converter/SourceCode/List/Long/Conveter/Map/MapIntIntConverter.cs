using System.Collections.Generic;

namespace Converter.List.Long
{
    /// 键值对 key => int, value => int
    public class MapIntIntConverter<TList> : IConverter<TList>
        where TList : IList<long>, new()
    {
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

        void IConverter<TList>.Clear()
        {
            map?.Clear();
        }

        public static implicit operator Dictionary<int, int>(MapIntIntConverter<TList> converter)
        {
            converter.map ??= new Dictionary<int, int>();
            return converter.map;
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