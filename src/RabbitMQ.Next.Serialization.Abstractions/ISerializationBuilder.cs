using System.Collections.Generic;

namespace RabbitMQ.Next.Serialization.Abstractions
{
    public interface ISerializationBuilder<out TBuilder>
    {
        TBuilder UseSerializer(ISerializer serializer, string contentType = null, bool isDefault = true);
    }
}