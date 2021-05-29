using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Channels
{
    internal class WaitMethodHandler : IMethodHandler
    {
        private readonly Action cancellationHandler;
        private readonly IMethodRegistry registry;
        private uint expectedMethodId;
        private TaskCompletionSource<IIncomingMethod> waitingTask;

        public WaitMethodHandler(IMethodRegistry registry, IChannelInternal channel)
        {
            this.registry = registry;
            this.cancellationHandler = () =>
            {
                this.waitingTask?.SetCanceled();
            };
            channel.Completion.ContinueWith(t =>
            {
                Exception ex = t.Exception?.InnerException;
                ex ??= new InvalidOperationException();
                this.waitingTask?.SetException(ex);
            });
        }

        public Task<IIncomingMethod> WaitAsync<TMethod>(CancellationToken cancellation = default)
            where TMethod : struct, IIncomingMethod
        {
            // todo: validate state, should probably throw if in wait state already
            var methodId = this.registry.GetMethodId<TMethod>();
            this.waitingTask = new TaskCompletionSource<IIncomingMethod>();
            this.expectedMethodId = methodId;
            if (cancellation != default)
            {
                cancellation.Register(this.cancellationHandler);
            }

            return this.waitingTask.Task;
        }

        ValueTask<bool> IMethodHandler.HandleAsync(IIncomingMethod method, IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
        {
            if (this.waitingTask == null)
            {
                return new ValueTask<bool>(false);
            }

            var task = this.waitingTask;
            if (method.Method != this.expectedMethodId)
            {
                return new ValueTask<bool>(false);
            }

            this.waitingTask = null;
            this.expectedMethodId = 0;
            task.SetResult(method);

            return new ValueTask<bool>(true);
        }
    }
}