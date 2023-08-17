using System;
using System.Collections.Generic;
using RabbitMQ.Next.Serialization.PlainText.Converters;

namespace RabbitMQ.Next.Serialization.PlainText;

internal sealed class PlainTextSerializerBuilder : IPlainTextSerializerBuilder
{
    private readonly Dictionary<Type, object> converters = new();

    public IReadOnlyDictionary<Type, object> Converters => this.converters;
    
    public IPlainTextSerializerBuilder UseConverter<T>(IConverter<T> converter)
    {
        this.converters[typeof(T)] = converter;
        
        return this;
    }

    public IPlainTextSerializerBuilder UseConverter<T>(IConverter<T> converter, bool useNullableWrapper) 
        where T : struct
    {
        this.converters[typeof(T)] = converter;
        if (useNullableWrapper)
        {
            this.converters[typeof(T?)] = new NullableConverterWrapper<T>(converter);
        }

        return this;
    }
}