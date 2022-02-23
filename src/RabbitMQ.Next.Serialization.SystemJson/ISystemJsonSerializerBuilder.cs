using System;
using System.Text.Json;

namespace RabbitMQ.Next.Serialization.SystemJson
{
    public interface ISystemJsonSerializerBuilder
    {
        ISystemJsonSerializerBuilder AsDefault();

        ISystemJsonSerializerBuilder ContentType(string contentType);

        ISystemJsonSerializerBuilder Configure(Action<JsonSerializerOptions> func);
    }
}