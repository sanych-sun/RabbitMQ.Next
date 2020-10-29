using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Abstractions.Channels
{
    public interface ISynchronizedChannel
    {
        Task SendAsync<TMethod>(TMethod request)
            where TMethod : struct, IOutgoingMethod;

        Task SendAsync<TMethod>(TMethod request, MessageProperties properties, ReadOnlySequence<byte> content)
            where TMethod : struct, IOutgoingMethod;

        Task<TMethod> WaitAsync<TMethod>(CancellationToken cancellation = default)
            where TMethod : struct, IIncomingMethod;
    }
}