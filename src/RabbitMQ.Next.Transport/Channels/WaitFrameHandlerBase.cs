using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next.Transport.Channels
{
    internal abstract class WaitFrameHandlerBase : FrameHandlerBase, IFrameHandler
    {
        private readonly SemaphoreSlim responseSync;

        private uint expectedMethodId;
        private IMethod methodArgs;

        protected WaitFrameHandlerBase(IMethodRegistry registry)
            : base(registry)
        {
            this.responseSync = new SemaphoreSlim(0);
        }

        protected async Task<IMethod> WaitAsyncInternal(uint methodId, CancellationToken cancellation = default)
        {
            // todo: validate state, should probably throw if in wait state already
            this.expectedMethodId = methodId;

            await this.responseSync.WaitAsync(cancellation);

            var result = this.methodArgs;
            this.methodArgs = null;
            this.expectedMethodId = 0;

            return result;
        }

        bool IFrameHandler.Handle(FrameType frameType, ReadOnlySequence<byte> payload)
        {
            if (this.expectedMethodId == 0)
            {
                return false;
            }

            payload = this.ReadMethodId(payload, out var methodId);

            if (methodId == this.expectedMethodId)
            {
                this.methodArgs = this.ParseMethodArguments(methodId, payload);
                this.responseSync.Release();

                return true;
            }

            // todo: throw connection exception here
            throw new InvalidOperationException();
        }
    }
}