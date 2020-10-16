using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next.Transport.Channels
{
    internal abstract class WaitFrameHandlerBase : FrameHandlerBase, IFrameHandler
    {
        private readonly Action cancellationHandler;
        private uint expectedMethodId;
        private TaskCompletionSource<IIncomingMethod> waitingTask;

        protected WaitFrameHandlerBase(IMethodRegistry registry)
            : base(registry)
        {
            this.cancellationHandler = this.CancelCurrentWaiting;
        }

        protected Task<IIncomingMethod> WaitAsyncInternal(uint methodId, CancellationToken cancellation = default)
        {
            // todo: validate state, should probably throw if in wait state already
            this.expectedMethodId = methodId;

            this.waitingTask = new TaskCompletionSource<IIncomingMethod>();
            if (cancellation != default)
            {
                cancellation.Register(this.cancellationHandler);
            }

            return this.waitingTask.Task;
        }

        bool IFrameHandler.Handle(FrameType frameType, ReadOnlySequence<byte> payload)
        {
            if (this.expectedMethodId == 0)
            {
                return false;
            }

            payload = this.ReadMethodId(payload, out var methodId);

            var task = this.waitingTask;
            if (methodId == this.expectedMethodId)
            {
                this.expectedMethodId = 0;
                this.waitingTask = null;

                task.SetResult(this.ParseMethodArguments(methodId, payload));

                return true;
            }

            if (methodId == (uint) MethodId.ChannelClose)
            {
                var channelClose = this.ParseArguments<Methods.Channel.CloseMethod>(payload);

                if (this.expectedMethodId == channelClose.FailedMethodId)
                {
                    task.SetException(new ChannelException(channelClose.StatusCode, channelClose.Description, channelClose.FailedMethodId));
                }
                else
                {
                    task.SetCanceled();
                }
            }

            return false;
        }

        private void CancelCurrentWaiting()
        {
            this.waitingTask?.SetCanceled();
        }
    }
}