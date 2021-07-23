using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Abstractions.Channels
{
    public interface IChannel
    {
        Task Completion { get; }

        ValueTask SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : struct, IOutgoingMethod;

        ValueTask<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : struct, IOutgoingMethod
            where TResponse : struct, IIncomingMethod;

        ValueTask SendAsync<TState>(TState state, Action<TState, IFrameBuilder> payload);

        ValueTask<TMethod> WaitAsync<TMethod>(CancellationToken cancellation = default)
            where TMethod : struct, IIncomingMethod;

        ValueTask CloseAsync(Exception ex = null);

        ValueTask CloseAsync(ushort statusCode, string description, MethodId failedMethodId);
    }
}