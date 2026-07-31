using System.Collections.Generic;

namespace Converter.List.Long
{
    /// 位图
    public class BitmapConverter<TList> : IConverter<TList>
        where TList: IList<long>, new()
    {
        private const int Size = sizeof(long) * 8;
        private TList source;
        
        void IConverter<TList>.Decode(TList source)
        {
            this.source?.Clear();
            if (source == null || source.Count == 0)
            {
                return;
            }
            this.source ??= new TList();
            for (int i = 0; i < source.Count; i++)
            {
                this.source.Add(source[i]);
            }
        }

        bool IConverter<TList>.TryEncode(out TList source)
        {
            if (this.source == null || this.source.Count == 0)
            {
                source = default;
                return false;
            }
            source = this.source;
            return true;
        }

        public void Clear()
        {
            source?.Clear();
        }

        public bool this[uint flag]
        {
            get => HasFlag(flag);
            set => SetFlag(flag, value);
        }      

        public bool HasFlag(uint flag)
        {
            if (source == null || source.Count == 0)
            {
                return false;
            }
            int flagValue = (int)flag;
            int arrayIndex = flagValue / Size;
            if (arrayIndex >= source.Count)
            {
                return false;
            }
            int unitBitIndex = flagValue % Size;
            long l = 1L << unitBitIndex;
            return (source[arrayIndex] & l) != 0;
        }
        
        public void SetFlag(uint flag, bool value)
        {
            if (!value)
            {
                RemoveFlag(flag);
            }
            else
            {
                AddFlag(flag);
            }
        }

        public void AddFlag(uint flag)
        {
            source ??= new TList();
            int flagValue = (int)flag;
            int arrayIndex = flagValue / Size;
            int unitBitIndex = flagValue % Size;
            // 不足补位
            for (int i = source.Count; i < arrayIndex + 1; i++)
            {
                source.Add(default);
            }
            long l = 1L << unitBitIndex;
            source[arrayIndex] |= l;
        }

        public void RemoveFlag(uint flag)
        {
            int flagValue = (int)flag;
            int arrayIndex = flagValue / Size;
            if (arrayIndex >= source.Count)
            {
                return;
            }
            int unitBitIndex = flagValue % Size;
            long l = 1L << unitBitIndex;
            source[arrayIndex] &= ~l;
            if (source[arrayIndex] != 0)
            {
                return;
            }
            // 检查 尾部 是否 有无用的 多余数据
            for (int i = source.Count - 1; i >= 0; i--)
            {
                if (source[i] != 0)
                {
                    return;
                } 
                source.RemoveAt(i);
            }
        }
    }
}