using System;
using System.Collections.Generic;

namespace Converter.List.Long
{
    /// 位图（时机复位）
    /// 在特定的时机重置
    /// 第一个数据 为 上次重置时机 的记录数据
    public abstract class BitmapTimingResetConverter<TList> : IConverter<TList>
        where TList: IList<long>, new()
    {
        private const int Size = sizeof(long) * 8;
        private BitmapConverter<TList> bitmapConverter;
        
        /// 重置的时机
        protected abstract Func<long, bool> TimingResetFunc { get; }
        /// 重置后的数据
        protected abstract Func<long> TimingAfterResetDataFunc { get; }
        /// 重置后的回调
        protected abstract Action TimingAfterResetCallBack { get; }
        
        void IConverter<TList>.Decode(TList source)
        {
            bitmapConverter?.Clear();
            if (source == null || source.Count == 0)
            {
                return;
            }
            bitmapConverter ??= new BitmapConverter<TList>();
            IConverter<TList> converter = bitmapConverter;
            if (!TimingResetFunc.Invoke(source[0]))
            {
                converter.Decode(source);
            }
            else
            {
                source.Clear();
                source.Add(TimingAfterResetDataFunc.Invoke());
                converter.Clear();
                converter.Decode(source);
                TimingAfterResetCallBack.Invoke();
            }
        }

        bool IConverter<TList>.TryEncode(out TList source)
        {
            if (bitmapConverter == null)
            {
                source = default;
                return false;
            }
            IConverter<TList> converter = bitmapConverter;
            return converter.TryEncode(out source);
        }

        public void Clear()
        {
            bitmapConverter?.Clear();
        }
        
        public bool this[uint flag]
        {
            get => HasFlag(flag);
            set => SetFlag(flag, value);
        }  
        
        public bool HasFlag(uint flag)
        {
            return bitmapConverter?.HasFlag(flag + Size) == true;
        }
        
        public void SetFlag(uint flag, bool value)
        {
            bitmapConverter?.SetFlag(flag + Size, value);
        }

        public void AddFlag(uint flag)
        {
            if (bitmapConverter == null)
            {
                bitmapConverter = new BitmapConverter<TList>();
                IConverter<TList> converter = bitmapConverter;
                converter.Decode(new TList
                {
                    TimingAfterResetDataFunc.Invoke(),
                });
            }
            bitmapConverter.AddFlag(flag + Size);
        }

        public void RemoveFlag(uint flag)
        {
            bitmapConverter?.RemoveFlag(flag + Size);
        }
    }
}