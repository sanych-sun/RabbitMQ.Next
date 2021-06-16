using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Tests
{
    internal class ChannelMock : IChannel
    {
        private readonly ISynchronizedChannel channel;
        private IReadOnlyList<IMethodHandler> handlers;

        public ChannelMock(ISynchronizedChannel channel)
        {
            this.Completion = new TaskCompletionSource<bool>().Task;
            this.channel = channel;
        }

        public bool IsClosed { get; private set; }

        public Task Completion { get; }

        public Task UseChannel(Func<ISynchronizedChannel, Task> fn, CancellationToken cancellation = default)
            => fn(this.channel);

        public Task UseChannel<TState>(TState state, Func<ISynchronizedChannel, TState, Task> fn, CancellationToken cancellation = default)
            => fn(this.channel, state);

        public Task<TResult> UseChannel<TResult>(Func<ISynchronizedChannel, Task<TResult>> fn, CancellationToken cancellation = default)
            => fn(this.channel);

        public Task<TResult> UseChannel<TState, TResult>(TState state, Func<ISynchronizedChannel, TState, Task<TResult>> fn, CancellationToken cancellation = default)
            => fn(this.channel, state);

        public Task CloseAsync(Exception ex = null)
        {
            this.IsClosed = true;
            return Task.CompletedTask;
        }

        public Task CloseAsync(ushort statusCode, string description, MethodId failedMethodId)
        {
            this.IsClosed = true;
            return Task.CompletedTask;
        }

        public void SetHandlers(IReadOnlyList<IMethodHandler> handlers)
        {
            this.handlers = handlers;
        }

        public async Task EmulateMethodAsync(IIncomingMethod method, IMessageProperties properties = null, ReadOnlySequence<byte> content = default)
        {
            foreach (var handler in this.handlers)
            {
                if (await handler.HandleAsync(method, properties, content))
                {
                    return;
                }
            }
        }
    }
}