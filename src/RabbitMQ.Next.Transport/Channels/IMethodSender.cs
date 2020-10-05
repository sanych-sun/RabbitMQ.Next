using System.Buffers;
using System.Threading.Tasks;
using RabbitMQ.Next.Transport.Methods;

namespace RabbitMQ.Next.Transport.Channels
{
    public interface IMethodSender
    {
        Task SendAsync<TMethod>(TMethod method)
            where TMethod : struct, IOutgoingMethod;

        Task SendAsync<TMethod>(TMethod method, ReadOnlySequence<byte> content)
            where TMethod : struct, IOutgoingMethod;
    }
}