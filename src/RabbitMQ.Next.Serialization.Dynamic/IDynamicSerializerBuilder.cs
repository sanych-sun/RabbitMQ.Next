using System;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Serialization.Dynamic;

public interface IDynamicSerializerBuilder
{
    public ISerializationBuilder<IDynamicSerializerBuilder> When(Func<IMessageProperties, bool> predicate);
}

