using System.Collections.Generic;

namespace Converter.List.Long
{
    /// 反转 键值对 key => int, value => bit (32bit)
    public class MapIntBitReverseConverter<TList> : IConverter<TList>
        where TList : IList<long>, new()
    {
        private MapIntBitConverter<TList> mapIntBitConverter = new MapIntBitConverter<TList>();
        
        void IConverter<TList>.Decode(TList source)
        {
            (mapIntBitConverter as IConverter<TList>).Decode(source);
        }

        bool IConverter<TList>.TryEncode(out TList source)
        {
            return (mapIntBitConverter as IConverter<TList>).TryEncode(out source);
        }

        void IConverter<TList>.Clear()
        {
            mapIntBitConverter.Clear();
        }

        public bool this[int key, uint flag]
        {
            get => HasFlag(key, flag);
            set => SetFlag(key, flag, value);
        }
        
        public void Fill()
        {
            mapIntBitConverter.Clear();
        }

        public bool ContainsKey(int key)
        {
            return !mapIntBitConverter.ContainsKey(key);
        }

        public bool HasFlag(int key, uint flag)
        {
            return !mapIntBitConverter.HasFlag(key, flag);
        }
        
        public void SetFlag(int key, uint flag, bool value)
        {
            mapIntBitConverter.SetFlag(key, flag, !value);
        }

        public void AddFlag(int key, uint flag)
        {
            mapIntBitConverter.RemoveFlag(key, flag);
        }

        public void RemoveFlag(int key, uint flag)
        {
            mapIntBitConverter.AddFlag(key, flag);
        }
    }
}