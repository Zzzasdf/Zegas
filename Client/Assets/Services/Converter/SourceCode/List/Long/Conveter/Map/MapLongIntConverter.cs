using System.Collections.Generic;

namespace Converter.List.Long
{
    /// 键值对 key => long, value => int
    /// 每 3 个数据 一个循环
    /// 第一个 数据 为 两个 value
    /// 后两位 数据 为 key
    /// 存储顺序 从低位开始
    public class MapLongIntConverter<TList> : IConverter<TList>
        where TList : IList<long>, new()
    {
        private Dictionary<long, int> map;
        private TList source;

        void IConverter<TList>.Decode(TList source)
        {
            map?.Clear();
            if (source == null || source.Count < 2)
            {
                return;
            }
            map ??= new Dictionary<long, int>();
            for (int i = 0; i + 2 < source.Count; i += 3)
            {
                (int high, int low) = LongConvertDoubleInt(source[i]);
                map.Add(source[i + 1], low);
                map.Add(source[i + 2], high);
            }
            // 最后一组 只有两个的情况
            if (source.Count % 3 == 2)
            {
                (_, int low) = LongConvertDoubleInt(source[^2]);
                map.Add(source[^1], low);
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
            int index = 0;
            foreach (var pair in map)
            {
                int valueIndex = index / 2 * 3;
                int keyIndex = valueIndex + 1 + (index & 1);
                for (int i = this.source.Count; i <= keyIndex; i++)
                {
                    this.source.Add(0);
                }
                this.source[keyIndex] = pair.Key;
                this.source[valueIndex] |= (index & 1) == 0 ? pair.Value : (long)pair.Value << 32;
                index++;
            }
            source = this.source;
            return true;
        }

        void IConverter<TList>.Clear()
        {
            map?.Clear();
        }
        
        public static implicit operator Dictionary<long, int>(MapLongIntConverter<TList> converter)
        {
            converter.map ??= new Dictionary<long, int>();
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