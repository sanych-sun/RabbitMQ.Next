using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Abstractions.Channels
{
    // TODO: need better name
    public interface IChannel : ISynchronizedChannel
    {
        bool IsClosed { get; }

        Task<TResult> UseSyncChannel<TResult, TState>(Func<ISynchronizedChannel, TState, Task<TResult>> fn, TState state);
    }
}