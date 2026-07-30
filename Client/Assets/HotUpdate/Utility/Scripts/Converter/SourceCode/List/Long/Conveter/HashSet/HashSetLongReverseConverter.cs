using System.Collections.Generic;

namespace Converter.List.Long
{
    /// 反转 HashSet ^ long
    public class HashSetLongReverseConverter<TList> : IConverter<TList>
        where TList : IList<long>, new()
    {
        private HashSetLongConverter<TList> hashSetLongConverter = new HashSetLongConverter<TList>();
        
        void IConverter<TList>.Decode(TList source)
        {
            (hashSetLongConverter as IConverter<TList>).Decode(source);
        }

        bool IConverter<TList>.TryEncode(out TList source)
        {
            return (hashSetLongConverter as IConverter<TList>).TryEncode(out source);
        }

        void IConverter<TList>.Clear()
        {
            HashSet<long> hashSet = hashSetLongConverter;
            hashSet.Clear();
        }
        
        public void Fill()
        {
            HashSet<long> hashSet = hashSetLongConverter;
            hashSet.Clear();
        }
        
        public bool Contains(long item)
        {
            HashSet<long> hashSet = hashSetLongConverter;
            return !hashSet.Contains(item);
        }

        public void Add(long item)
        {
            HashSet<long> hashSet = hashSetLongConverter;
            hashSet.Remove(item);
        }

        public void Remove(long item)
        {
            HashSet<long> hashSet = hashSetLongConverter;
            hashSet.Add(item);
        }
    }
}
