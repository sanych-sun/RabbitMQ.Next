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
                var task = Interlocked.Exchange(ref this.waitingTask, null);
                task?.TrySetCanceled();
            };

            channel.Completion.ContinueWith(t =>
            {
                var task = Interlocked.Exchange(ref this.waitingTask, null);
                if (task == null)
                {
                    return;
                }

                var ex = t.Exception?.InnerException ?? t.Exception;

                if (ex == null)
                {
                    task.TrySetCanceled();
                }
                else
                {
                    task.TrySetException(ex);
                }
            });
        }

        public async Task<IIncomingMethod> WaitAsync<TMethod>(CancellationToken cancellation = default)
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
            Interlocked.Exchange(ref this.waitingTask, new TaskCompletionSource<IIncomingMethod>(TaskCreationOptions.RunContinuationsAsynchronously));
            this.expectedMethodId = methodId;
            CancellationTokenRegistration cancellationTokenRegistration = default;
            if (cancellation.CanBeCanceled)
            {
                cancellationTokenRegistration = cancellation.Register(this.cancellationHandler);
            }

            var result = await this.waitingTask.Task;
            await cancellationTokenRegistration.DisposeAsync();
            return result;
        }

        ValueTask<bool> IMethodHandler.HandleAsync(IIncomingMethod method, IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
        {
            if (method.MethodId != this.expectedMethodId)
            {
                return new ValueTask<bool>(false);
            }


            this.expectedMethodId = 0;
            var task = Interlocked.Exchange(ref this.waitingTask, null);
            task?.SetResult(method);

            return new ValueTask<bool>(true);
        }
    }
}