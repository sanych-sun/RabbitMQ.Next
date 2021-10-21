using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Channels
{
    internal interface IMethodSender
    {
        ValueTask SendAsync<TRequest>(TRequest request, CancellationToken cancellation = default)
            where TRequest : struct, IOutgoingMethod;

        ValueTask PublishAsync<TState>(TState state, string exchange, string routingKey, IMessageProperties properties, Action<TState, IBufferWriter<byte>> payload, PublishFlags flags = PublishFlags.None, CancellationToken cancellation = default);
    }
}