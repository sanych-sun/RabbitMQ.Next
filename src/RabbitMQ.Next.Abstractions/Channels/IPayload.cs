using System;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Channels;

public interface IPayload: IMessageProperties, IContentAccessor, IDisposable
{
}