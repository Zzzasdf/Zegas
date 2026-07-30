using System.Collections.Generic;

namespace Converter.List.Long
{
    /// 键值对 key => int, value => long
    /// 每 3 个数据 一个循环
    /// 第一个 数据 为 两个 key
    /// 后两位 数据 为 value
    /// 存储顺序 从低位开始
    public class MapIntLongConverter<TList> : IConverter<TList>
        where TList : IList<long>, new()
    {
        private Dictionary<int, long> map;
        private TList source;

        void IConverter<TList>.Decode(TList source)
        {
            map?.Clear();
            if (source == null || source.Count < 2)
            {
                return;
            }
            map ??= new Dictionary<int, long>();
            for (int i = 0; i + 2 < source.Count; i += 3)
            {
                (int high, int low) = LongConvertDoubleInt(source[i]);
                map.Add(low, source[i + 1]);
                map.Add(high, source[i + 2]);
            }
            // 最后一组 只有两个的情况
            if (source.Count % 3 == 2)
            {
                (_, int low) = LongConvertDoubleInt(source[^2]);
                map.Add(low, source[^1]);
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
                int keyIndex = index / 2 * 3;
                int valueIndex = keyIndex + 1 + (index & 1);
                for (int i = this.source.Count; i <= valueIndex; i++)
                {
                    this.source.Add(0);
                }
                this.source[keyIndex] |= (index & 1) == 0 ? pair.Key : (long)pair.Key << 32;
                this.source[valueIndex] = pair.Value;
                index++;
            }
            source = this.source;
            return true;
        }

        void IConverter<TList>.Clear()
        {
            map?.Clear();
        }
        
        public static implicit operator Dictionary<int, long>(MapIntLongConverter<TList> converter)
        {
            converter.map ??= new Dictionary<int, long>();
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