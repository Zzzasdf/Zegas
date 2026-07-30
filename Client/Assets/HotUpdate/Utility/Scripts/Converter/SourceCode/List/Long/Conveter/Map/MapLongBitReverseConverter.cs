using System.Collections.Generic;

namespace Converter.List.Long
{
    /// 反转 键值对 key => long, value => bit (64bit)
    public class MapLongBitReverseConverter<TList> : IConverter<TList>
        where TList : IList<long>, new()
    {
        private MapLongBitConverter<TList> mapLongBitConverter = new MapLongBitConverter<TList>();
        
        void IConverter<TList>.Decode(TList source)
        {
            (mapLongBitConverter as IConverter<TList>).Decode(source);
        }

        bool IConverter<TList>.TryEncode(out TList source)
        {
            return (mapLongBitConverter as IConverter<TList>).TryEncode(out source);
        }

        void IConverter<TList>.Clear()
        {
            mapLongBitConverter.Clear();
        }
        
        public bool this[long key, uint flag]
        {
            get => HasFlag(key, flag);
            set => SetFlag(key, flag, value);
        }
        
        public void Fill()
        {
            mapLongBitConverter.Clear();
        }

        public bool ContainsKey(long key)
        {
            return !mapLongBitConverter.ContainsKey(key);
        }

        public bool HasFlag(long key, uint flag)
        {
            return !mapLongBitConverter.HasFlag(key, flag);
        }
        
        public void SetFlag(long key, uint flag, bool value)
        {
            mapLongBitConverter.SetFlag(key, flag, !value);
        }

        public void AddFlag(long key, uint flag)
        {
            mapLongBitConverter.RemoveFlag(key, flag);
        }

        public void RemoveFlag(long key, uint flag)
        {
            mapLongBitConverter.AddFlag(key, flag);
        }
    }
}
