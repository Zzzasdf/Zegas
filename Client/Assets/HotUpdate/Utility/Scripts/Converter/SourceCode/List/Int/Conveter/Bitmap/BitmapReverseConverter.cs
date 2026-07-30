using System.Collections.Generic;

namespace Converter.List.Int
{
    /// 反转 位图
    public class BitmapReverseConverter<TList> : IConverter<TList>
        where TList: IList<int>, new()
    {
        private BitmapConverter<TList> bitmapConverter = new BitmapConverter<TList>();

        void IConverter<TList>.Decode(TList source)
        {
            (bitmapConverter as IConverter<TList>).Decode(source);
        }

        bool IConverter<TList>.TryEncode(out TList source)
        {
            return (bitmapConverter as IConverter<TList>).TryEncode(out source);
        }

        void IConverter<TList>.Clear()
        {
            bitmapConverter.Clear();
        }

        public void Fill()
        {
            bitmapConverter.Clear();
        }

        public bool this[uint flag]
        {
            get => HasFlag(flag);
            set => SetFlag(flag, value);
        }      
        
        public bool HasFlag(uint flag)
        {
            return !bitmapConverter.HasFlag(flag);
        }

        public void SetFlag(uint flag, bool value)
        {
            bitmapConverter.SetFlag(flag, !value);
        }

        public void AddFlag(uint flag)
        {
            bitmapConverter.RemoveFlag(flag);
        }

        public void RemoveFlag(uint flag)
        {
            bitmapConverter.AddFlag(flag);
        }

        public (int index, int value) GetListIndexDataByFlag(uint flag)
        {
            return bitmapConverter.GetListIndexDataByFlag(flag);
        }
    }
}