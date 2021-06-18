using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Transport.Methods.Basic;
using RabbitMQ.Next.Transport.Methods.Channel;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher
{
    public class ConfirmMethodHandlerTests
    {
        [Fact]
        public async Task HandlesAckMethod()
        {
            IMethodHandler handler = new ConfirmMethodHandler();
            var handled = await handler.HandleAsync(new AckMethod(1, false), null, ReadOnlySequence<byte>.Empty);
            Assert.True(handled);
        }

        [Fact]
        public async Task HandlesNackMethod()
        {
            IMethodHandler handler = new ConfirmMethodHandler();
            var handled = await handler.HandleAsync(new NackMethod(1, false, false), null, ReadOnlySequence<byte>.Empty);
            Assert.True(handled);
        }

        [Theory]
        [MemberData(nameof(IgnoreMethodsTestCases))]
        public async Task IgnoresOtherMethods(IIncomingMethod method)
        {
            IMethodHandler handler = new ConfirmMethodHandler();
            var handled = await handler.HandleAsync(method, null, ReadOnlySequence<byte>.Empty);
            Assert.False(handled);
        }

        [Fact]
        public async Task AskSingleMessage()
        {
            var handler = new ConfirmMethodHandler();

            var wait = handler.WaitForConfirmAsync(1);
            await Task.Delay(10);
            Assert.False(wait.IsCompleted);

            await AckMessageAsync(handler, 1, true);

            await Task.Delay(10);
            Assert.True(await wait);
        }

        [Fact]
        public async Task AskMultipleMessages()
        {
            var handler = new ConfirmMethodHandler();

            var wait1 = handler.WaitForConfirmAsync(1);
            var wait2 = handler.WaitForConfirmAsync(2);
            var wait3 = handler.WaitForConfirmAsync(3);
            await Task.Delay(10);
            Assert.False(wait1.IsCompleted);
            Assert.False(wait2.IsCompleted);
            Assert.False(wait3.IsCompleted);

            await AckMessageAsync(handler, 2, true, true);

            await Task.Delay(10);
            Assert.True(wait1.IsCompleted);
            Assert.True(wait2.IsCompleted);
            Assert.False(wait3.IsCompleted);

            Assert.True(await wait1);
            Assert.True(await wait2);
        }

        [Fact]
        public async Task NackSingleMessage()
        {
            var handler = new ConfirmMethodHandler();

            var wait = handler.WaitForConfirmAsync(1);
            await Task.Yield();
            Assert.False(wait.IsCompleted);

            await AckMessageAsync(handler, 1, false);

            await Task.Delay(10);
            Assert.False(await wait);
        }

        [Fact]
        public async Task NackMultipleMessages()
        {
            var handler = new ConfirmMethodHandler();

            var wait1 = handler.WaitForConfirmAsync(1);
            var wait2 = handler.WaitForConfirmAsync(2);
            var wait3 = handler.WaitForConfirmAsync(3);
            await Task.Delay(10);
            Assert.False(wait1.IsCompleted);
            Assert.False(wait2.IsCompleted);
            Assert.False(wait3.IsCompleted);

            await AckMessageAsync(handler, 2, false, true);

            await Task.Delay(50);
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
            var handler = new ConfirmMethodHandler();

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

        private static ValueTask<bool> AckMessageAsync(IMethodHandler handler, ulong deliveryTag, bool ack, bool multiple = false)
        {
            if (ack)
            {
                return handler.HandleAsync(new AckMethod(deliveryTag, multiple), null, ReadOnlySequence<byte>.Empty);
            }

            return handler.HandleAsync(new NackMethod(deliveryTag, multiple, false), null, ReadOnlySequence<byte>.Empty);
        }

        public static IEnumerable<object[]> IgnoreMethodsTestCases()
        {
            yield return new Object[] {new GetOkMethod("a", null, 1, false, 10)};

            yield return new Object[] {new GetEmptyMethod()};

            yield return new Object[] {new CloseMethod(200, null, MethodId.Unknown)};
        }
    }
}