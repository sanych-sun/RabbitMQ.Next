using System;
using System.Buffers;
using System.Collections.Generic;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Serialization.PlainText;

internal class PlainTextSerializer : ISerializer
{
    private readonly IReadOnlyDictionary<Type, object> converters;

    public PlainTextSerializer(IReadOnlyDictionary<Type, object> converters)
    {
        if (converters == null || converters.Count == 0)
        {
            throw new ArgumentNullException(nameof(converters));
        }
        
        foreach (var (key, converter) in converters)
        {
            var converterInterface = typeof(IConverter<>).MakeGenericType(key);
            if (!converter.GetType().IsAssignableTo(converterInterface))
            {
                throw new ArgumentException($"Wrongly configured converter for {key} type.");
            }
        }
        
        this.converters = converters;
    }

    public void Serialize<TContent>(IMessageProperties message, TContent content, IBufferWriter<byte> writer)
        => this.GetConverter<TContent>().Format(content, writer);

    public TContent Deserialize<TContent>(IMessageProperties message, ReadOnlySequence<byte> bytes)
        => this.GetConverter<TContent>().Parse(bytes);

    private IConverter<TContent> GetConverter<TContent>()
    {
        if (this.converters.TryGetValue(typeof(TContent), out var converter))
        {
            return (IConverter<TContent>)converter;
        }

        throw new NotSupportedException($"Cannot resolve converter for the type: {typeof(TContent).FullName}");
    }
}