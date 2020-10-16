using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Abstractions.Channels
{
    public interface IChannel
    {
        Task SendAsync<TMethod>(TMethod request, ReadOnlySequence<byte> content = default)
            where TMethod : struct, IOutgoingMethod;

        Task<TMethod> WaitAsync<TMethod>(CancellationToken cancellation = default)
            where TMethod : struct, IIncomingMethod;

        Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, ReadOnlySequence<byte> content = default, CancellationToken cancellation = default)
            where TRequest : struct, IOutgoingMethod
            where TResponse : struct, IIncomingMethod;
    }
}