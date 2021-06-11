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
        private readonly IChannel channel;
        private MethodId expectedMethodId;
        private TaskCompletionSource<IIncomingMethod> waitingTask;

        public WaitMethodHandler(IMethodRegistry registry, IChannel channel)
        {
            this.registry = registry;
            this.channel = channel;
            this.cancellationHandler = () =>
            {
                this.waitingTask?.SetCanceled();
            };

            channel.Completion.ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    Exception ex = t.Exception;
                    if (ex is AggregateException aggregateException && aggregateException.InnerException != null)
                    {
                        ex = aggregateException.InnerException;
                    }

                    this.waitingTask?.SetException(ex);
                }
                else
                {
                    this.waitingTask?.SetCanceled();
                }

                this.waitingTask = null;
            });
        }

        public Task<IIncomingMethod> WaitAsync<TMethod>(CancellationToken cancellation = default)
            where TMethod : struct, IIncomingMethod
        {
            if (this.channel.Completion.IsCompleted)
            {
                throw new InvalidOperationException("Cannot wait on completed channel");
            }

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