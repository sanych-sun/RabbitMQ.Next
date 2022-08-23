using System;
using System.Buffers;

namespace RabbitMQ.Next.Messaging;

public interface IPayload: IMessageProperties, IDisposable
{
    ReadOnlySequence<byte> GetBody();
}