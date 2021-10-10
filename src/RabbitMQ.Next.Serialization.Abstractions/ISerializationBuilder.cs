using System.Collections.Generic;

namespace RabbitMQ.Next.Serialization.Abstractions
{
    public interface ISerializationBuilder<out TBuilder>
    {
        TBuilder UseSerializer(ISerializer serializer, IReadOnlyList<string> contentTypes = null, bool isDefault = true);
    }
}