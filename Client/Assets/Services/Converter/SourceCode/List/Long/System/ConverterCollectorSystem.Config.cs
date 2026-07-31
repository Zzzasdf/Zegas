using System;
using System.Collections.Generic;
using UnityEngine;

namespace Converter.List.Long
{
    public partial class ConverterCollectorSystem<TListList, TList>
    {
        /// 收集器类型
        protected abstract Dictionary<Type, IConverterCollector<TList>> converterCollectors { get; }
        
        /// 配置表
        protected abstract Dictionary<int, Type> config { get; }

        /// 获取收集器类型
        private bool TryGetTypeConfig(int typeValue, out IConverterCollector<TList> converterCollector)
        {
            if (!config.TryGetValue(typeValue, out Type type))
            {
                Debug.LogError($"缺少对应的配置 => {typeValue}");
                converterCollector = default;
                return false;
            }
            if (!converterCollectors.TryGetValue(type, out IConverterCollector<TList> value))
            {
                Debug.LogError($"缺少对应的收集器类型 => {type}");
                converterCollector = default;
                return false;
            }
            converterCollector = value;
            return true;
        }
        
        /// 获取收集器类型
        private bool TryGetTypeConfig<TConverterCollector, TConverter>(int typeValue, out TConverterCollector converterCollector)
            where TConverterCollector: class, IConverterCollector<TConverter, TList>
            where TConverter : class, IConverter<TList>, new()
        {
            if (!config.TryGetValue(typeValue, out Type type))
            {
                Debug.LogError($"缺少对应的配置 => {typeValue}");
                converterCollector = default;
                return false;
            }
            if (!converterCollectors.TryGetValue(type, out IConverterCollector<TList> value))
            {
                Debug.LogError($"缺少对应的收集器类型 => {type}");
                converterCollector = default;
                return false;
            }
            converterCollector = value as TConverterCollector;
            return true;
        }
    }
}