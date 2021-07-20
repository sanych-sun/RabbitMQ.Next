using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Tests.Mocks
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

        public ValueTask SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : struct, IOutgoingMethod => this.channel.SendAsync(request);

        public ValueTask SendAsync<TState>(TState state, Action<TState, IFrameBuilder> payload) => throw new NotImplementedException();

        public ValueTask UseChannel(Func<ISynchronizedChannel, ValueTask> fn, CancellationToken cancellation = default)
            => fn(this.channel);

        public ValueTask UseChannel<TState>(TState state, Func<ISynchronizedChannel, TState, ValueTask> fn, CancellationToken cancellation = default)
            => fn(this.channel, state);

        public ValueTask<TResult> UseChannel<TResult>(Func<ISynchronizedChannel, ValueTask<TResult>> fn, CancellationToken cancellation = default)
            => fn(this.channel);

        public ValueTask<TResult> UseChannel<TState, TResult>(TState state, Func<ISynchronizedChannel, TState, ValueTask<TResult>> fn, CancellationToken cancellation = default)
            => fn(this.channel, state);

        public ValueTask CloseAsync(Exception ex = null)
        {
            this.IsClosed = true;
            return default;
        }

        public ValueTask CloseAsync(ushort statusCode, string description, MethodId failedMethodId)
        {
            this.IsClosed = true;
            return default;
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