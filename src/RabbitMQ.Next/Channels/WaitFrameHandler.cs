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
    internal class WaitFrameHandler : IFrameHandler
    {
        private readonly Action cancellationHandler;
        private readonly IMethodRegistry registry;
        private readonly IChannel channel;
        private MethodId expectedMethodId;
        private TaskCompletionSource<IIncomingMethod> waitingTask;

        public WaitFrameHandler(IMethodRegistry registry, IChannel channel)
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
            Interlocked.Exchange(ref this.waitingTask, new TaskCompletionSource<IIncomingMethod>(TaskCreationOptions.RunContinuationsAsynchronously));
            this.expectedMethodId = methodId;
            CancellationTokenRegistration cancellationTokenRegistration = default;
            if (cancellation.CanBeCanceled)
            {
                cancellationTokenRegistration = cancellation.Register(this.cancellationHandler);
            }

            this.waitingTask.Task.ContinueWith(x => cancellationTokenRegistration.Dispose());
            return this.waitingTask.Task;

        }

        public ValueTask<bool> HandleMethodFrameAsync(MethodId methodId, ReadOnlyMemory<byte> payload)
        {
            if (methodId != this.expectedMethodId)
            {
                return new(false);
            }

            this.expectedMethodId = 0;
            var task = Interlocked.Exchange(ref this.waitingTask, null);
            var method = this.registry.GetParser(methodId).ParseMethod(payload);
            task?.SetResult(method);

            return new ValueTask<bool>(true);
        }

        public ValueTask<bool> HandleContentAsync(IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
            => new(false);
    }
}