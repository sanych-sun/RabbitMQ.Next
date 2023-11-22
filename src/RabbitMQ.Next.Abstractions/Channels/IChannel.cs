using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Channels;

public interface IChannel
{
    IDisposable WithMessageHandler<TMethod>(IMessageHandler<TMethod> handler)
        where TMethod: struct, IIncomingMethod;
        
    Task Completion { get; }

    ValueTask SendAsync<TRequest>(TRequest request, CancellationToken cancellation = default)
        where TRequest : struct, IOutgoingMethod;

    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellation = default)
        where TRequest : struct, IOutgoingMethod
        where TResponse : struct, IIncomingMethod;

    ValueTask<ulong> PublishAsync<TContent>(string exchange, string routingKey, TContent content, IMessageProperties properties, PublishFlags flags = PublishFlags.None, CancellationToken cancellation = default);

    Task<TMethod> WaitAsync<TMethod>(CancellationToken cancellation = default)
        where TMethod : struct, IIncomingMethod;

    Task CloseAsync(Exception ex = null);

    Task CloseAsync(ushort statusCode, string description, MethodId failedMethodId);
}