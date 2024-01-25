using System;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Channels;

public interface IPayload: IMessageProperties, IContentAccessor, IDisposable
{
}
