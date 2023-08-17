using System.Buffers;

namespace RabbitMQ.Next.Serialization.PlainText.Converters;

internal class NullableConverterWrapper<T> : IConverter<T?>
    where T : struct

{
    private readonly IConverter<T> wrappedConverter;
    
    public NullableConverterWrapper(IConverter<T> wrappedConverter)
    {
        this.wrappedConverter = wrappedConverter;
    }
    
    public void Format(T? content, IBufferWriter<byte> writer)
    {
        if (!content.HasValue)
        {
            return;
        }
        
        this.wrappedConverter.Format(content.Value, writer);
    }

    public T? Parse(ReadOnlySequence<byte> bytes)
    {
        if (bytes.IsEmpty)
        {
            return null;
        }

        return this.wrappedConverter.Parse(bytes);
    }
}