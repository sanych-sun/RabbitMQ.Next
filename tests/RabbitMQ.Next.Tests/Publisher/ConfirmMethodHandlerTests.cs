using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Registry;
using Xunit;
using BitConverter = RabbitMQ.Next.Transport.BitConverter;

namespace RabbitMQ.Next.Tests.Publisher
{
    public class ConfirmMethodHandlerTests
    {
        private readonly IMethodRegistry registry = new MethodRegistryBuilder().AddBasicMethods().Build();

        [Fact]
        public async Task HandlesAckMethod()
        {
            IFrameHandler handler = new ConfirmFrameHandler(this.registry);
            var handled = await AckMessageAsync(handler, 1, true);
            Assert.True(handled);
        }

        [Fact]
        public async Task HandlesNackMethod()
        {
            IFrameHandler handler = new ConfirmFrameHandler(this.registry);
            var handled = await AckMessageAsync(handler, 1, false);
            Assert.True(handled);
        }

        [Fact]
        public async Task IgnoresOtherMethods()
        {
            IFrameHandler handler = new ConfirmFrameHandler(this.registry);
            var handled = await handler.HandleMethodFrameAsync(MethodId.BasicDeliver, ReadOnlyMemory<byte>.Empty);
            Assert.False(handled);
        }

        [Fact]
        public async Task IgnoresContent()
        {
            IFrameHandler handler = new ConfirmFrameHandler(this.registry);
            var handled = await handler.HandleContentAsync(Substitute.For<IMessageProperties>(), ReadOnlySequence<byte>.Empty);
            Assert.False(handled);
        }

        [Fact]
        public async Task AckSingleMessage()
        {
            var handler = new ConfirmFrameHandler(this.registry);

            var wait = handler.WaitForConfirmAsync(1);
            Assert.False(wait.IsCompleted);

            await AckMessageAsync(handler, 1, true);

            Assert.True(await wait);
        }

        [Fact]
        public async Task AckMultipleMessages()
        {
            var handler = new ConfirmFrameHandler(this.registry);

            var wait1 = handler.WaitForConfirmAsync(1);
            var wait2 = handler.WaitForConfirmAsync(2);
            var wait3 = handler.WaitForConfirmAsync(3);
            Assert.False(wait1.IsCompleted);
            Assert.False(wait2.IsCompleted);
            Assert.False(wait3.IsCompleted);

            await AckMessageAsync(handler, 2, true, true);

            Assert.True(wait1.IsCompleted);
            Assert.True(wait2.IsCompleted);
            Assert.False(wait3.IsCompleted);

            Assert.True(await wait1);
            Assert.True(await wait2);
        }

        [Fact]
        public async Task NackSingleMessage()
        {
            var handler = new ConfirmFrameHandler(this.registry);

            var wait = handler.WaitForConfirmAsync(1);
            await Task.Yield();
            Assert.False(wait.IsCompleted);

            await AckMessageAsync(handler, 1, false);

            Assert.False(await wait);
        }

        [Fact]
        public async Task NackMultipleMessages()
        {
            var handler = new ConfirmFrameHandler(this.registry);

            var wait1 = handler.WaitForConfirmAsync(1);
            var wait2 = handler.WaitForConfirmAsync(2);
            var wait3 = handler.WaitForConfirmAsync(3);

            Assert.False(wait1.IsCompleted);
            Assert.False(wait2.IsCompleted);
            Assert.False(wait3.IsCompleted);

            await AckMessageAsync(handler, 2, false, true);

            Assert.True(wait1.IsCompleted);
            Assert.True(wait2.IsCompleted);
            Assert.False(wait3.IsCompleted);

            Assert.False(await wait1);
            Assert.False(await wait2);
        }

        [Theory]
        [InlineData(100, 2)]
        [InlineData(1000, 10)]
        public async Task AckConcurrent(int messages, int threads)
        {
            var handler = new ConfirmFrameHandler(this.registry);

            var waitTasks = Enumerable.Range(1, messages)
                .Select(i => handler.WaitForConfirmAsync((ulong)i).AsTask())
                .ToArray();

            var confirms = new ConcurrentQueue<ulong>();
            for (var i = 1; i <= messages; i++)
            {
                confirms.Enqueue((ulong)i);
            }

            await Task.WhenAll(Enumerable.Range(1, threads).Select(_ => Task.Run(() =>
            {
                while (confirms.TryDequeue(out var deliveryTag))
                {
                    AckMessageAsync(handler, deliveryTag, true);
                }
            })).ToArray());

            var results = await Task.WhenAll(waitTasks);
            Assert.True(results.All(r => r));
        }

        private static ValueTask<bool> AckMessageAsync(IFrameHandler handler, ulong deliveryTag, bool ack, bool multiple = false)
        {
            var memory = new byte[9];

            if (ack)
            {
                ((Memory<byte>) memory)
                    .Write(deliveryTag)
                    .Write(multiple);
                return handler.HandleMethodFrameAsync(MethodId.BasicAck, memory);
            }

            ((Memory<byte>) memory)
                .Write(deliveryTag)
                .Write(BitConverter.ComposeFlags(multiple, false));

            return handler.HandleMethodFrameAsync(MethodId.BasicNack, memory);
        }
    }
}