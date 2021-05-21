using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Abstractions.Channels
{
    public interface IChannel : ISynchronizedChannel
    {
        Task Completion { get; }

        Task UseSyncChannel<TState>(TState state, Func<ISynchronizedChannel, TState, Task> fn);

        Task<TResult> UseSyncChannel<TResult, TState>(TState state, Func<ISynchronizedChannel, TState, Task<TResult>> fn);

        Task CloseAsync();

        Task CloseAsync(ushort statusCode, string description, uint failedMethodId);
    }
}