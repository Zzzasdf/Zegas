using System.Collections.Generic;

namespace Converter.List.Long
{
    public interface IConverterCollector<TList>
        where TList: IList<long>, new()
    {
        /// 清空数据
        public void Clear();
        
        /// 添加源数据
        public void AddSource(int typeValue, TList source);
        
        /// 获取转换器集合字典
        public bool TryGetConverterMap(out Dictionary<int, IConverter<TList>> converterMap);
    }
    
    public interface IConverterCollector<out TConverter, in TList>
        where TConverter : class, IConverter<TList>, new()
        where TList: IList<long>, new()
    {
        /// 获取转换器
        public TConverter GetConverter(int typeValue);
    }
}
