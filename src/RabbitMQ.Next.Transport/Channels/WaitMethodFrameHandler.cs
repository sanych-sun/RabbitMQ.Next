using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next.Transport.Channels
{
    internal class WaitMethodFrameHandler : FrameHandlerBase, IFrameHandler
    {
        private readonly Action cancellationHandler;
        private uint expectedMethodId;
        private TaskCompletionSource<IIncomingMethod> waitingTask;

        public WaitMethodFrameHandler(IMethodRegistry registry)
            : base(registry)
        {
            this.cancellationHandler = () =>
            {
                this.waitingTask?.SetCanceled();
            };
        }

        public Task<IIncomingMethod> WaitAsync(uint methodId, CancellationToken cancellation = default)
        {
            // todo: validate state, should probably throw if in wait state already
            this.waitingTask = new TaskCompletionSource<IIncomingMethod>();
            this.expectedMethodId = methodId;
            if (cancellation != default)
            {
                cancellation.Register(this.cancellationHandler);
            }

            return this.waitingTask.Task;
        }

        bool IFrameHandler.Handle(FrameType frameType, ReadOnlySequence<byte> payload)
        {
            if (this.waitingTask == null)
            {
                return false;
            }

            payload = this.ReadMethodId(payload, out var methodId);

            var task = this.waitingTask;
            if (methodId == this.expectedMethodId)
            {
                this.waitingTask = null;
                this.expectedMethodId = 0;
                task.SetResult(this.ParseMethodArguments(methodId, payload));

                return true;
            }

            if (methodId == (uint) MethodId.ChannelClose)
            {
                var channelClose = this.ParseArguments<Methods.Channel.CloseMethod>(payload);
                task.SetException(new ChannelException(channelClose.StatusCode, channelClose.Description, channelClose.FailedMethodId));
            }

            return false;
        }
    }
}