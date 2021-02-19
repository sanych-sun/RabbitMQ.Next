using System.Buffers;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Channels
{
    public interface IFrameSender
    {
        Task SendMethodAsync<TMethod>(TMethod method)
            where TMethod : struct, IOutgoingMethod;

        Task SendContentHeaderAsync(IMessageProperties properties, ulong contentSize);

        Task SendContentAsync(ReadOnlySequence<byte> contentBytes);
    }
}