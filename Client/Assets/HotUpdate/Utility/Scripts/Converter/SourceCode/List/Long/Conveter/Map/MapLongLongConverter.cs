using System.Collections.Generic;

namespace Converter.List.Long
{
    /// 键值对 key => long, value => long
    public class MapLongLongConverter<TList> : IConverter<TList>
        where TList : IList<long>, new()
    {
        private Dictionary<long, long> map;
        private TList source;

        void IConverter<TList>.Decode(TList source)
        {
            map?.Clear();
            if (source == null || source.Count < 2)
            {
                return;
            }
            map ??= new Dictionary<long, long>();
            for (int i = 0; i + 1 < source.Count; i += 2)
            {
                map.Add(source[i], source[i + 1]);
            }
        }

        bool IConverter<TList>.TryEncode(out TList source)
        {
            if (map == null || map.Count == 0)
            {
                source = default;
                return false;
            }
            this.source ??= new TList();
            this.source.Clear();
            foreach (var pair in map)
            {
                this.source.Add(pair.Key);
                this.source.Add(pair.Value);
            }
            source = this.source;
            return true;
        }

        void IConverter<TList>.Clear()
        {
            map?.Clear();
        }

        public static implicit operator Dictionary<long, long>(MapLongLongConverter<TList> converter)
        {
            converter.map ??= new Dictionary<long, long>();
            return converter.map;
        }
    }
}