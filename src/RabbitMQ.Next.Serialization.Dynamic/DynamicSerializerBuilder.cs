using System;
using System.Collections.Generic;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Serialization.Dynamic;

internal class DynamicSerializerBuilder: IDynamicSerializerBuilder, ISerializationBuilder<IDynamicSerializerBuilder>
{
    private Func<IMessageProperties, bool> currentPredicate;
    
    public List<(Func<IMessageProperties, bool> predicate, ISerializer serializer)> Serializers { get; } = new();

    public ISerializationBuilder<IDynamicSerializerBuilder> When(Func<IMessageProperties, bool> predicate)
    {
        if (this.currentPredicate != null)
        {
            throw new InvalidOperationException();
        }

        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }
        
        this.currentPredicate = predicate;
        return this;
    }

    public IDynamicSerializerBuilder UseSerializer(ISerializer serializer)
    {
        if (this.currentPredicate == null)
        {
            throw new InvalidOperationException();
        }

        if (serializer == null)
        {
            throw new ArgumentNullException(nameof(serializer));
        }
        
        this.Serializers.Add((this.currentPredicate, serializer));
        this.currentPredicate = null;
        return this;
    }
}