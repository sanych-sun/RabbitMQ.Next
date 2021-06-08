using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Channels
{
    internal class WaitMethodHandler : IMethodHandler
    {
        private readonly Action cancellationHandler;
        private readonly IMethodRegistry registry;
        private MethodId expectedMethodId;
        private TaskCompletionSource<IIncomingMethod> waitingTask;

        public WaitMethodHandler(IMethodRegistry registry)
        {
            this.registry = registry;
            this.cancellationHandler = () =>
            {
                this.waitingTask?.SetCanceled();
            };
        }

        public void Dispose()
        {
            this.waitingTask?.SetCanceled();
        }

        public Task<IIncomingMethod> WaitAsync<TMethod>(CancellationToken cancellation = default)
            where TMethod : struct, IIncomingMethod
        {
            if (this.expectedMethodId != 0)
            {
                throw new InvalidOperationException("Cannot enter wait state: already in wait state.");
            }

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
            if (method.MethodId != this.expectedMethodId)
            {
                return new ValueTask<bool>(false);
            }

            var task = this.waitingTask;
            this.waitingTask = null;
            this.expectedMethodId = 0;
            task.SetResult(method);

            return new ValueTask<bool>(true);
        }
    }
}