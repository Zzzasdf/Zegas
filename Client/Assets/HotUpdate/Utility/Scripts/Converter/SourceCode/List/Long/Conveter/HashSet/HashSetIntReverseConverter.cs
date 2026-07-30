using System.Collections.Generic;

namespace Converter.List.Long
{
    /// 反转 HashSet ^ int
    public class HashSetIntReverseConverter<TList> : IConverter<TList>
        where TList : IList<long>, new()
    {
        private HashSetIntConverter<TList> hashSetIntConverter = new HashSetIntConverter<TList>();

        void IConverter<TList>.Decode(TList source)
        {
            (hashSetIntConverter as IConverter<TList>).Decode(source);
        }

        bool IConverter<TList>.TryEncode(out TList source)
        {
            return (hashSetIntConverter as IConverter<TList>).TryEncode(out source);
        }

        void IConverter<TList>.Clear()
        {
            HashSet<int> hashSet = hashSetIntConverter;
            hashSet.Clear();
        }

        public void Fill()
        {
            HashSet<int> hashSet = hashSetIntConverter;
            hashSet.Clear();
        }
        
        public bool Contains(int item)
        {
            HashSet<int> hashSet = hashSetIntConverter;
            return !hashSet.Contains(item);
        }

        public void Add(int item)
        {
            HashSet<int> hashSet = hashSetIntConverter;
            hashSet.Remove(item);
        }

        public void Remove(int item)
        {
            HashSet<int> hashSet = hashSetIntConverter;
            hashSet.Add(item);
        }
    }
}
