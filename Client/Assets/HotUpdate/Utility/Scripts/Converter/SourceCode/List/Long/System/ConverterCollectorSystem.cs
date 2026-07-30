using System;
using System.Collections.Generic;
using UnityEngine;

namespace Converter.List.Long
{
    public abstract partial class ConverterCollectorSystem<TListList, TList>: IConverterCollectorSystem<TListList, TList>
        where TListList: IList<TList>, new()
        where TList: IList<long>, new()
    {
        private Dictionary<int, TList> sourceMap;
        private TListList source;

        public void AddSource(TListList source)
        {
            if (source == null || source.Count == 0)
            {
                return;
            }
            // 重置数据
            foreach (var item in converterCollectors)
            {
                item.Value.Clear();
            }
            for (int i = 0; i < source.Count; i++)
            {
                TList dataSource = source[i];
                if (!TryGetTypeConfig(i, out IConverterCollector<TList> converterCollector))
                {
                    Debug.LogError($"缺少对应的类型解析器 typeValue => {i}");
                    continue;
                }
                converterCollector.AddSource(i, dataSource);
            }
        }

        bool IConverterCollectorSystem<TListList, TList>.TryEncodeMap(out Dictionary<int, TList> sourceMap)
        {
            this.sourceMap ??= new Dictionary<int, TList>();
            this.sourceMap.Clear();
            foreach (var pair in config)
            {
                Type type = pair.Value;
                if (!converterCollectors.TryGetValue(type, out IConverterCollector<TList> value))
                {
                    Debug.LogError($"缺少对应类型的收集器配置 => {type}");
                    sourceMap = null;
                    continue;
                }
                if (!value.TryGetConverterMap(out Dictionary<int, IConverter<TList>> converterMap))
                {
                    // 没有数据，无需获取
                    continue;
                }
                foreach (var converterPair in converterMap)
                {
                    int key = converterPair.Key;
                    IConverter<TList> converter = converterPair.Value;
                    if (!converter.TryEncode(out TList source))
                    {
                        // 没有数据，无需获取
                        continue;
                    }
                    this.sourceMap.Add(key, source);
                }
            }
            if (this.sourceMap.Count == 0)
            {
                sourceMap = null;
                return false;
            }
            sourceMap = this.sourceMap;
            return true;
        }

        public bool TryEncode(int typeValue, out TList source)
        {
            if (!config.TryGetValue(typeValue, out Type type))
            {
                source = default;
                return false;
            }
            if (!converterCollectors.TryGetValue(type, out IConverterCollector<TList> value))
            {
                Debug.LogError($"缺少对应类型的收集器配置 => {type}");
                source = default;
                return false;
            }
            if (!value.TryGetConverterMap(out Dictionary<int, IConverter<TList>> converterMap))
            {
                // 没有数据，无需获取
                source = default;
                return false;
            }
            if (!converterMap.TryGetValue(typeValue, out IConverter<TList> converter))
            {
                // 没有数据，无需获取
                source = default;
                return false;
            }
            return converter.TryEncode(out source);
        }

        public bool TryEncode(out TListList source)
        {
            this.source ??= new TListList();
            this.source.Clear();
            foreach (var pair in config)
            {
                Type type = pair.Value;
                if (!converterCollectors.TryGetValue(type, out IConverterCollector<TList> value))
                {
                    Debug.LogError($"缺少对应类型的收集器配置 => {type}");
                    continue;
                }
                if (!value.TryGetConverterMap(out Dictionary<int, IConverter<TList>> converterMap))
                {
                    // 没有数据，无需获取
                    continue;
                }
                foreach (var converterPair in converterMap)
                {
                    int key = converterPair.Key;
                    IConverter<TList> converter = converterPair.Value;
                    if (!converter.TryEncode(out TList encodeSource))
                    {
                        // 没有数据，无需获取
                        continue;
                    }
                    // 长度不足，补充元素
                    for (int i = this.source.Count; i <= key; i++)
                    {
                        this.source.Add(default);
                    }
                    this.source[key] = encodeSource;
                }
            }
            if (this.source.Count == 0)
            {
                source = default;
                return false;
            }
            source = this.source;
            return true;
        }

        public bool TryGetConverter<TConverterCollector, TConverter>(int typeValue, out TConverter converter)
            where TConverterCollector : class, IConverterCollector<TConverter, TList>
            where TConverter : class, IConverter<TList>, new()
        {
            if (!TryGetTypeConfig<TConverterCollector, TConverter>(typeValue, out TConverterCollector converterCollector))
            {
                converter = default;
                return false;
            }
            converter = converterCollector.GetConverter(typeValue);
            return true;
        }

        public abstract void Save(int typeValue);

        public abstract void SaveAll();
    }
}