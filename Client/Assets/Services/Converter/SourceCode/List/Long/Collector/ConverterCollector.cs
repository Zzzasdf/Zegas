using System.Collections.Generic;
using UnityEngine;

namespace Converter.List.Long
{
    public class ConverterCollector<TConverter, TList>: IConverterCollector<TConverter, TList>, IConverterCollector<TList>
        where TConverter: class, IConverter<TList>, new()
        where TList: IList<long>, new() 
    {
        private Dictionary<int, IConverter<TList>> converterMap;
        
        TConverter IConverterCollector<TConverter, TList>.GetConverter(int typeValue)
        {
            converterMap ??= new Dictionary<int, IConverter<TList>>();
            if (!converterMap.TryGetValue(typeValue, out IConverter<TList> converter))
            {
                converterMap.Add(typeValue, converter = new TConverter());
            }
            return converter as TConverter;
        }
        
        void IConverterCollector<TList>.Clear()
        {
            converterMap?.Clear();
        }
        
        void IConverterCollector<TList>.AddSource(int typeValue, TList source)
        {
            converterMap ??= new Dictionary<int, IConverter<TList>>();
            if (converterMap.ContainsKey(typeValue))
            {
                Debug.LogError($"已存在相同 typeValue 的转换器：{typeValue}");
                return;
            }
            TConverter converter = new TConverter();
            converter.Decode(source);
            converterMap.Add(typeValue, converter);
        }
        
        bool IConverterCollector<TList>.TryGetConverterMap(out Dictionary<int, IConverter<TList>> converterMap)
        {
            if (this.converterMap == null)
            {
                converterMap = default;
                return false;
            }
            converterMap = this.converterMap;
            return true;
        }
    }
}
