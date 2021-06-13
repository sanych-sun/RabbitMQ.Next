using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Abstractions.Channels
{
    public interface IChannel
    {
        Task Completion { get; }

        Task UseChannel(Func<ISynchronizedChannel, Task> fn, CancellationToken cancellation = default);

        Task UseChannel<TState>(TState state, Func<ISynchronizedChannel, TState, Task> fn, CancellationToken cancellation = default);

        Task<TResult> UseChannel<TResult>(Func<ISynchronizedChannel, Task<TResult>> fn, CancellationToken cancellation = default);

        Task<TResult> UseChannel<TState, TResult>(TState state, Func<ISynchronizedChannel, TState, Task<TResult>> fn, CancellationToken cancellation = default);

        Task CloseAsync(Exception ex = null);

        Task CloseAsync(ushort statusCode, string description, MethodId failedMethodId);
    }
}