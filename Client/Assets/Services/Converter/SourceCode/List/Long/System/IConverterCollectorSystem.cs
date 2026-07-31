using System.Collections.Generic;

namespace Converter.List.Long
{
    public interface IConverterCollectorSystem<TListList, TList>
        where TListList: IList<TList>, new()
        where TList: IList<long>, new()
    {
        /// 添加源数据
        void AddSource(TListList source);

        /// 编码字典
        bool TryEncodeMap(out Dictionary<int, TList> sourceMap);

        /// 编码
        bool TryEncode(int typeValue, out TList source);

        /// 编码
        bool TryEncode(out TListList source);
        
        /// 获取转换器
        bool TryGetConverter<TConverterCollector, TConverter>(int typeValue, out TConverter converter) 
            where TConverterCollector : class, IConverterCollector<TConverter, TList>
            where TConverter : class, IConverter<TList>, new();

        /// 保存单体
        void Save(int typeValue);

        /// 保存所有
        void SaveAll();
    }
}
