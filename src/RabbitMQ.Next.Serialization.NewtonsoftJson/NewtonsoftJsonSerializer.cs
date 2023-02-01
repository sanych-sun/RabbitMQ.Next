using System.Buffers;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Serialization.NewtonsoftJson;

internal class NewtonsoftJsonSerializer : ISerializer
{
    private readonly JsonSerializer serializer;
    private readonly Encoding encoding;

    public NewtonsoftJsonSerializer(JsonSerializerSettings options, Encoding encoding)
    {
        this.encoding = encoding;
        this.serializer = JsonSerializer.Create(options);
    }

    public void Serialize<TContent>(IMessageProperties properties, TContent content, IBufferWriter<byte> writer)
    {
        using var textWriter = new TextWriterWrapper(this.encoding, writer);
        using var jsonWriter = new JsonTextWriter(textWriter);

        this.serializer.Serialize(jsonWriter, content);
    }

    public TContent Deserialize<TContent>(IMessageProperties properties, ReadOnlySequence<byte> bytes)
    {
        if (bytes.IsEmpty)
        {
            return default;
        }

        using var textReader = new TextReaderWrapper(this.encoding, bytes);
        using var jsonReader = new JsonTextReader(textReader);

        return this.serializer.Deserialize<TContent>(jsonReader);
    }
}