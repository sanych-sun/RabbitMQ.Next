using System.Collections.Generic;

namespace RabbitMQ.Next.Serialization.Abstractions
{
    public interface ISerializationBuilder
    {
        void AddSerializer(ISerializer serializer, params string[] contentTypes);
    }
}