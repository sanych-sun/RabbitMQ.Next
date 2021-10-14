using System.Collections.Generic;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization
{
    public interface ISerializerFactory
    {
        void RegisterSerializer(ISerializer serializer, IReadOnlyList<string> contentTypes = null, bool isDefault = true);

        ISerializer Get(string contentType);
    }
}