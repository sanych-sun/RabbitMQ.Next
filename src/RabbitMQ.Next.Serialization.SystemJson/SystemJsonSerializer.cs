﻿using System.Buffers;
using System.Text.Json;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Serialization.SystemJson;

internal class SystemJsonSerializer : ISerializer
{
    private readonly JsonSerializerOptions options;

    public SystemJsonSerializer(JsonSerializerOptions options)
    {
        this.options = options;
    }

    public void Serialize<TContent>(IMessageProperties properties, TContent content, IBufferWriter<byte> writer)
    {
        var jsonWriter = new Utf8JsonWriter(writer);
        JsonSerializer.Serialize(jsonWriter, content, this.options);
    }

    public TContent Deserialize<TContent>(IMessageProperties properties, ReadOnlySequence<byte> bytes)
    {
        if (bytes.IsEmpty)
        {
            return default;
        }

        var reader = new Utf8JsonReader(bytes);
        return JsonSerializer.Deserialize<TContent>(ref reader, this.options);
    }
}