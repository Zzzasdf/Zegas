using System.Collections.Generic;

namespace Converter.List.Long
{
    /// int 集合
    /// 第一个 long 只存 最后一个高位 是否有效的 状态
    public class ListIntConverter<TList, TListInt> : IConverter<TList>
        where TList : IList<long>, new()
        where TListInt : IList<int>, new()
    {
        private TListInt list;
        private TList source;

        void IConverter<TList>.Decode(TList source)
        {
            list?.Clear();
            if (source == null || source.Count < 2)
            {
                return;
            }
            list ??= new TListInt();
            for (int i = 1; i < source.Count - 1; i++)
            {
                (int high, int low) = LongConvertDoubleInt(source[i]);
                list.Add(low);
                list.Add(high);
            }
            // 最后一位需要判断 高位 是否有效
            bool validLastHigh = source[0] == 1;
            {
                (int high, int low) = LongConvertDoubleInt(source[^1]);
                list.Add(low);
                if (validLastHigh)
                {
                    list.Add(high);
                }
            }
        }

        bool IConverter<TList>.TryEncode(out TList source)
        {
            if (list == null || list.Count == 0)
            {
                source = default;
                return false;
            }
            this.source ??= new TList();
            this.source.Clear();
            this.source.Add((list.Count & 1) == 0 ? 1 : 0);
            for (int i = 0; i + 1 < list.Count; i += 2)
            {
                this.source.Add(DoubleIntCombineLong(list[i + 1], list[i]));
            }
            if ((list.Count & 1) != 0)
            {
                this.source.Add(list[^1]);
            }
            source = this.source;
            return true;
        }

        void IConverter<TList>.Clear()
        {
            list?.Clear();
        }

        public static implicit operator TListInt(ListIntConverter<TList, TListInt> converter)
        {
            converter.list ??= new TListInt();
            return converter.list;
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