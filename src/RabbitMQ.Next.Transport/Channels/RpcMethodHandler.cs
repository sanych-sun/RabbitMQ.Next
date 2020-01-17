using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next.Transport.Channels
{
    internal class RpcMethodHandler : WaitFrameHandlerBase
    {
        private readonly IMethodSender methodSender;

        public RpcMethodHandler(IMethodRegistry registry, IMethodSender methodSender)
            : base(registry)
        {
            this.methodSender = methodSender;
        }

        public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, ReadOnlySequence<byte> content = default, CancellationToken cancellation = default)
            where TRequest : struct, IOutgoingMethod
            where TResponse: struct, IIncomingMethod
        {
            var responseMethodId = this.Registry.GetMethodId<TResponse>();

            await this.methodSender.SendAsync(request, content);
            var result = await this.WaitAsyncInternal(responseMethodId, cancellation);

            return (TResponse) result;
        }
    }
}