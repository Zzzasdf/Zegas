using Converter.List.Long;
using Google.Protobuf.Collections;

public class ConverterCollector<TConverter> : ConverterCollector<TConverter, RepeatedField<long>>
    where TConverter:  class, IConverter<RepeatedField<long>>, new()
{
    
}
