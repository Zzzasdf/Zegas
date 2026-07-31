// using System;
// using System.Collections.Generic;
// using System.Text;
// using Converter.List.Long;
// using Google.Protobuf.Collections;
// using UnityEngine;
//
// /// 服务端配置 的 转换系统
// public class ServerConverterCollectorSystem : ConverterCollectorSystem<RepeatedField<RepeatedField<long>>, RepeatedField<long>>, IConverterCollectorSystemManager
// {
//     /// 收集器类型
//     protected override Dictionary<Type, IConverterCollector<RepeatedField<long>>> converterCollectors { get; } = new()
//     {
//         // Bitmap
//         [typeof(EBitmap)] = new ConverterCollector<BitmapConverter>(),
//         [typeof(EBitmapReverse)] = new ConverterCollector<BitmapReverseConverter>(),
//         
//         // List
//         [typeof(EListInt)] = new ConverterCollector<ListIntConverter>(),
//         [typeof(EListLong)] = new ConverterCollector<ListLongConverter>(),
//         
//         // HashSet
//         [typeof(EHashSetInt)] = new ConverterCollector<HashSetIntConverter>(),
//         [typeof(EHashSetIntReverse)] = new ConverterCollector<HashSetIntReverseConverter>(),
//         [typeof(EHashSetLong)] = new ConverterCollector<HashSetLongConverter>(),
//         [typeof(EHashSetLongReverse)] = new ConverterCollector<HashSetLongReverseConverter>(),
//         
//         // Map
//         [typeof(EMapIntBit)] = new ConverterCollector<MapIntBitConverter>(),
//         [typeof(EMapIntBitReverse)] = new ConverterCollector<MapIntBitReverseConverter>(),
//         [typeof(EMapIntInt)] = new ConverterCollector<MapIntIntConverter>(),
//         [typeof(EMapIntLong)] = new ConverterCollector<MapIntLongConverter>(),
//         [typeof(EMapLongBit)] = new ConverterCollector<MapLongBitConverter>(),
//         [typeof(EMapLongBitReverse)] = new ConverterCollector<MapLongBitReverseConverter>(),
//         [typeof(EMapLongInt)] = new ConverterCollector<MapLongIntConverter>(),
//         [typeof(EMapLongLong)] = new ConverterCollector<MapLongLongConverter>(),
//
//         // Custom
//     };
//
//     /// 配置表
//     protected override Dictionary<int, Type> config { get; } = new()
//     {
//         [(int)EBitmap.Test_0] = typeof(EBitmap),
//         [(int)EBitmapReverse.Test_1] = typeof(EBitmapReverse),
//         [(int)EListInt.Test_2] = typeof(EListInt),
//         [(int)EListLong.Test_3] = typeof(EListLong),
//         [(int)EHashSetInt.Test_4] = typeof(EHashSetInt),
//         [(int)EHashSetIntReverse.Test_5] = typeof(EHashSetIntReverse),
//         [(int)EHashSetLong.Test_6] = typeof(EHashSetLong),
//         [(int)EHashSetLongReverse.Test_7] = typeof(EHashSetLongReverse),
//         [(int)EMapIntBit.Test_8] = typeof(EMapIntBit),
//         [(int)EMapIntBitReverse.Test_9] = typeof(EMapIntBitReverse),
//         [(int)EMapIntInt.Test_10] = typeof(EMapIntInt),
//         [(int)EMapIntLong.Test_11] = typeof(EMapIntLong),
//         [(int)EMapLongBit.Test_12] = typeof(EMapLongBit),
//         [(int)EMapLongBitReverse.Test_13] = typeof(EMapLongBitReverse),
//         [(int)EMapLongInt.Test_14] = typeof(EMapLongInt),
//         [(int)EMapLongLong.Test_15] = typeof(EMapLongLong),
//     };
//     
//     void IInit.OnInit()
//     {
//     }
//     void IReset.OnReset()
//     {
//     }
//     void IDestroy.OnDestroy()
//     {
//     }
//     
//     public override void Save(int typeValue)
//     {
//         Debug.LogError($"保存类型 => {typeValue}");
//         if (!TryEncode(typeValue, out RepeatedField<long> source))
//         {
//             Debug.LogError("保存点");
//             return;
//         }
//         Debug.LogError("保存点");
//     }
//
//     public override void SaveAll()
//     {
//         Debug.LogError("保存所有");
//         if (!TryEncode(out RepeatedField<RepeatedField<long>> source))
//         {
//             Debug.LogError("保存点");
//             return;
//         }
//         Debug.LogError("保存点");
//     }
//
//     public override string ToString()
//     {
//         if (!TryEncode(out RepeatedField<RepeatedField<long>> source))
//         {
//             return string.Empty;
//         }
//         StringBuilder sb = new StringBuilder();
//         for (int i = 0; i < source.Count; i++)
//         {
//             var tmpSource = source[i];
//             sb.AppendLine($"类型 => {i}");
//             for (int j = 0; j < tmpSource?.Count; j++)
//             {
//                 sb.AppendLine($"[{j}] => {tmpSource[j]}");
//             }
//         }
//         return sb.ToString();
//     }
//
//     private enum UniqueRefCollector
//     {
//         Test_0 = 0,
//         Test_1 = 1,
//         Test_2 = 2,
//         Test_3 = 3,
//         Test_4 = 4,
//         Test_5 = 5,
//         Test_6 = 6,
//         Test_7 = 7,
//         Test_8 = 8,
//         Test_9 = 9,
//         Test_10 = 10,
//         Test_11 = 11,
//         Test_12 = 12,
//         Test_13 = 13,
//         Test_14 = 14,
//         Test_15 = 15,
//     }
//
//     #region Bitmap
//     public enum EBitmap { Test_0 = UniqueRefCollector.Test_0 }
//     public enum EBitmapReverse { Test_1 = UniqueRefCollector.Test_1, }
//     #endregion
//
//     #region List
//     public enum EListInt { Test_2 = UniqueRefCollector.Test_2, }
//     public enum EListLong { Test_3 = UniqueRefCollector.Test_3, }
//     #endregion
//
//     #region HashSet
//     public enum EHashSetInt { Test_4 = UniqueRefCollector.Test_4, }
//     public enum EHashSetIntReverse { Test_5 = UniqueRefCollector.Test_5, }
//     public enum EHashSetLong { Test_6 = UniqueRefCollector.Test_6, }
//     public enum EHashSetLongReverse { Test_7 = UniqueRefCollector.Test_7, }
//     #endregion
//
//     #region Map
//     public enum EMapIntBit { Test_8 = UniqueRefCollector.Test_8, }
//     public enum EMapIntBitReverse { Test_9 = UniqueRefCollector.Test_9, }
//     public enum EMapIntInt { Test_10 = UniqueRefCollector.Test_10, }
//     public enum EMapIntLong { Test_11 = UniqueRefCollector.Test_11, }
//     public enum EMapLongBit { Test_12 = UniqueRefCollector.Test_12, }
//     public enum EMapLongBitReverse { Test_13 = UniqueRefCollector.Test_13, }
//     public enum EMapLongInt { Test_14 = UniqueRefCollector.Test_14, }
//     public enum EMapLongLong { Test_15 = UniqueRefCollector.Test_15, }
//     #endregion
//
//     #region Custom
//     #endregion
//
//     #region Converter
//     public BitmapConverter GetConverter(EBitmap self) => GetConverter<ConverterCollector<BitmapConverter>, BitmapConverter>((int)self);
//     public BitmapReverseConverter GetConverter(EBitmapReverse self) => GetConverter<ConverterCollector<BitmapReverseConverter>, BitmapReverseConverter>((int)self);
//     public RepeatedField<int> GetConverter(EListInt self) => GetConverter<ConverterCollector<ListIntConverter>, ListIntConverter>((int)self);
//     public RepeatedField<long> GetConverter(EListLong self) => GetConverter<ConverterCollector<ListLongConverter>, ListLongConverter>((int)self);
//     public HashSet<int> GetConverter(EHashSetInt self) => GetConverter<ConverterCollector<HashSetIntConverter>, HashSetIntConverter>((int)self);
//     public HashSetIntReverseConverter GetConverter(EHashSetIntReverse self) => GetConverter<ConverterCollector<HashSetIntReverseConverter>, HashSetIntReverseConverter>((int)self);
//     public HashSetLongConverter GetConverter(EHashSetLong self) => GetConverter<ConverterCollector<HashSetLongConverter>, HashSetLongConverter>((int)self);
//     public HashSetLongReverseConverter GetConverter(EHashSetLongReverse self) => GetConverter<ConverterCollector<HashSetLongReverseConverter>, HashSetLongReverseConverter>((int)self);
//     public MapIntBitConverter GetConverter(EMapIntBit self) => GetConverter<ConverterCollector<MapIntBitConverter>, MapIntBitConverter>((int)self);
//     public MapIntBitReverseConverter GetConverter(EMapIntBitReverse self) => GetConverter<ConverterCollector<MapIntBitReverseConverter>, MapIntBitReverseConverter>((int)self);
//     public Dictionary<int, int> GetConverter(EMapIntInt self) => GetConverter<ConverterCollector<MapIntIntConverter>, MapIntIntConverter>((int)self);
//     public Dictionary<int, long> GetConverter(EMapIntLong self) => GetConverter<ConverterCollector<MapIntLongConverter>, MapIntLongConverter>((int)self);
//     public MapLongBitConverter GetConverter(EMapLongBit self) => GetConverter<ConverterCollector<MapLongBitConverter>, MapLongBitConverter>((int)self);
//     public MapLongBitReverseConverter GetConverter(EMapLongBitReverse self) => GetConverter<ConverterCollector<MapLongBitReverseConverter>, MapLongBitReverseConverter>((int)self);
//     public Dictionary<long, int> GetConverter(EMapLongInt self) => GetConverter<ConverterCollector<MapLongIntConverter>, MapLongIntConverter>((int)self);
//     public Dictionary<long, long> GetConverter(EMapLongLong self) => GetConverter<ConverterCollector<MapLongLongConverter>, MapLongLongConverter>((int)self);
//     #endregion
//     
//     private TConverter GetConverter<TCollector, TConverter>(int self)
//         where TCollector: ConverterCollector<TConverter, RepeatedField<long>>
//         where TConverter: class, IConverter<RepeatedField<long>>, new()
//     {
//         if(!TryGetConverter<TCollector, TConverter>(self, out TConverter converter))
//         {
//             throw new Exception("配置有问题，请检查");
//         }
//         return converter;
//     }
// }