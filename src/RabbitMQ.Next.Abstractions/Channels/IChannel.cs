using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Abstractions.Channels
{
    // TODO: need better name
    public interface IChannel : ISynchronizedChannel
    {
        bool IsClosed { get; }

        Task<TResult> UseSyncChannel<TResult, TState>(Func<ISynchronizedChannel, TState, Task<TResult>> fn, TState state);

        Task CloseAsync();

        Task CloseAsync(ushort statusCode, string description, uint failedMethodId);
    }
}