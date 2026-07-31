using System.Collections.Generic;

namespace Converter.List.Long
{
    /// HashSet ^ long
    public class HashSetLongConverter<TList> : IConverter<TList>
        where TList : IList<long>, new()
    {
        private HashSet<long> hashSet;
        
        void IConverter<TList>.Decode(TList source)
        {
            hashSet?.Clear();
            if (source == null || source.Count == 0)
            {
                return;
            }
            hashSet ??= new HashSet<long>();
            foreach (var item in source)
            {
                hashSet.Add(item);
            }
        }

        bool IConverter<TList>.TryEncode(out TList source)
        {
            if (hashSet == null || hashSet.Count == 0)
            {
                source = default;
                return false;
            }
            source = new TList();
            foreach (var item in hashSet)
            {
                source.Add(item);
            }
            return true;
        }

        void IConverter<TList>.Clear()
        {
            hashSet?.Clear();
        }

        public static implicit operator HashSet<long>(HashSetLongConverter<TList> converter)
        {
            converter.hashSet ??= new HashSet<long>();
            return converter.hashSet;
        }
    }
}