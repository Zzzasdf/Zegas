using System.Collections.Generic;

namespace Converter.List.Long
{
    /// long 集合
    public class ListLongConverter<TList> : IConverter<TList>
        where TList : IList<long>, new()
    {
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

        void IConverter<TList>.Clear()
        {
            source?.Clear();
        }
        
        public static implicit operator TList(ListLongConverter<TList> converter)
        {
            converter.source ??= new TList();
            return converter.source;
        }
    }
}