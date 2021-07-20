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

        ValueTask SendAsync<TState>(TState state, Action<TState, IFrameBuilder> payload);

        ValueTask UseChannel(Func<ISynchronizedChannel, ValueTask> fn, CancellationToken cancellation = default);

        ValueTask UseChannel<TState>(TState state, Func<ISynchronizedChannel, TState, ValueTask> fn, CancellationToken cancellation = default);

        ValueTask<TResult> UseChannel<TResult>(Func<ISynchronizedChannel, ValueTask<TResult>> fn, CancellationToken cancellation = default);

        ValueTask<TResult> UseChannel<TState, TResult>(TState state, Func<ISynchronizedChannel, TState, ValueTask<TResult>> fn, CancellationToken cancellation = default);

        ValueTask CloseAsync(Exception ex = null);

        ValueTask CloseAsync(ushort statusCode, string description, MethodId failedMethodId);
    }
}