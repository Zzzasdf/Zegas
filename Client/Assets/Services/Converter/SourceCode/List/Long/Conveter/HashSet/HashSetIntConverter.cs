using System.Collections.Generic;

namespace Converter.List.Long
{
    /// HashSet ^ int
    /// 第一个 long 只存 最后一个高位 是否有效的 状态
    public class HashSetIntConverter<TList> : IConverter<TList>
        where TList : IList<long>, new()
    {
        private HashSet<int> hashSet;
        private TList source;

        void IConverter<TList>.Decode(TList source)
        {
            hashSet?.Clear();
            if (source == null || source.Count < 2)
            {
                return;
            }
            hashSet ??= new HashSet<int>();
            for (int i = 1; i < source.Count - 1; i++)
            {
                (int high, int low) = LongConvertDoubleInt(source[i]);
                hashSet.Add(low);
                hashSet.Add(high);
            }
            // 最后一位需要判断 高位 是否有效
            bool validLastHigh = source[0] == 1;
            {
                (int high, int low) = LongConvertDoubleInt(source[^1]);
                hashSet.Add(low);
                if (validLastHigh)
                {
                    hashSet.Add(high);
                }
            }
        }

        bool IConverter<TList>.TryEncode(out TList source)
        {
            if (hashSet == null || hashSet.Count == 0)
            {
                source = default;
                return false;
            }
            this.source ??= new TList();
            this.source.Clear();
            this.source.Add((hashSet.Count & 1) == 0 ? 1 : 0);

            int index = 0;
            int recordLastItem = 0;
            foreach (var item in hashSet)
            {
                if (index % 2 == 0)
                {
                    recordLastItem = item;
                    index++;
                    continue;
                }
                this.source.Add(DoubleIntCombineLong(recordLastItem, item));
                index++;
            }
            if ((hashSet.Count & 1) != 0)
            {
                this.source.Add(recordLastItem);
            }
            source = this.source;
            return true;
        }

        void IConverter<TList>.Clear()
        {
            hashSet?.Clear();
        }

        public static implicit operator HashSet<int>(HashSetIntConverter<TList> converter)
        {
            converter.hashSet ??= new HashSet<int>();
            return converter.hashSet;
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