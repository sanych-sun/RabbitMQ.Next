using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next.Transport.Channels
{
    internal class WaitMethodFrameHandler : WaitFrameHandlerBase
    {
        public WaitMethodFrameHandler(IMethodRegistry registry)
            : base(registry)
        {}

        public async Task<TMethod> WaitAsync<TMethod>(CancellationToken cancellation)
            where TMethod : struct, IIncomingMethod
        {
            var methodId = this.Registry.GetMethodId<TMethod>();
            var result = await this.WaitAsyncInternal(methodId, cancellation);

            return (TMethod) result;
        }
    }
}