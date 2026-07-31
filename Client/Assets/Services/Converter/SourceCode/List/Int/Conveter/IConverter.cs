using System.Collections.Generic;

namespace Converter.List.Int
{
    public interface IConverter<TList>
        where TList: IList<int>, new()
    {
        /// 解码
        void Decode(TList source);

        /// 编码
        bool TryEncode(out TList source);

        /// 清空
        void Clear();
    }
}