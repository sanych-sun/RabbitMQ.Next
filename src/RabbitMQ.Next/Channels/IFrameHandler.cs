using System;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Channels;

internal interface IFrameHandler
{
    void Release(Exception ex);

    FrameType AcceptFrame(FrameType type, MemoryBlock payload);
}