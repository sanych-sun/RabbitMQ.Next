using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Transport
{
    public interface ISocketWriter
    {
        Task SendAsync<TState>(TState state, Func<Func<ReadOnlyMemory<byte>, ValueTask>, TState, ValueTask> writer, CancellationToken cancellation = default);
    }

    public static class SocketWriterExtensions
    {
        public static Task SendAsync(this ISocketWriter socketWriter, ReadOnlyMemory<byte> payload, CancellationToken cancellation = default)
            => socketWriter.SendAsync(payload, (writer, data) => writer(data), cancellation);
    }
}