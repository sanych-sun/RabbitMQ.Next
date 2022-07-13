using System.Collections.Generic;

namespace RabbitMQ.Next.Serialization.MessagePack;

internal class MessagePackSerializerBuilder: IMessagePackSerializerBuilder
{
    private static readonly string[] DefaultContentTypes = { "application/msgpack", "application/x-msgpack" };
    private List<string> contentTypes;

    public bool IsDefault { get; private set; } = true;

    public IReadOnlyList<string> ContentTypes
        => this.contentTypes == null ? DefaultContentTypes : this.contentTypes;

    public IMessagePackSerializerBuilder AsDefault()
    {
        this.IsDefault = true;
        return this;
    }

    public IMessagePackSerializerBuilder ContentType(string contentType)
    {
        this.contentTypes ??= new List<string>(DefaultContentTypes);
        this.contentTypes.Add(contentType);

        return this;
    }
}