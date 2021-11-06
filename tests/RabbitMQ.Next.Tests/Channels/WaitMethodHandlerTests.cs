using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Basic;
using RabbitMQ.Next.Transport.Methods.Registry;
using Xunit;

namespace RabbitMQ.Next.Tests.Channels
{
    public class WaitMethodHandlerTests
    {
        [Fact]
        public async Task WaitThrowsOnCompletedChannel()
        {
            var handler = this.CreateHandler(Task.CompletedTask);

            await Assert.ThrowsAsync<InvalidOperationException>(async() => await handler.WaitAsync<AckMethod>());
        }

        [Fact]
        public async Task WaitThrowsOnChannelError()
        {
            var tcs = new TaskCompletionSource<bool>();
            var handler = this.CreateHandler(tcs.Task);

            var wait = handler.WaitAsync<AckMethod>();
            var ex = new ArgumentNullException();
            tcs.SetException(ex);

            await Assert.ThrowsAsync<ArgumentNullException>(async() => await wait);
        }

        [Fact]
        public async Task WaitCancelsOnChannelCompletion()
        {
            var tcs = new TaskCompletionSource<bool>();
            var handler = this.CreateHandler(tcs.Task);

            var wait = handler.WaitAsync<AckMethod>();
            tcs.SetResult(true);

            await Assert.ThrowsAsync<TaskCanceledException>(async() => await wait);
        }

        [Fact]
        public async Task CancelsOnCancellationToken()
        {
            var handler = this.CreateHandler();
            var cancellation = new CancellationTokenSource();

            var wait = handler.WaitAsync<AckMethod>(cancellation.Token);
            Assert.False(wait.IsCanceled);

            cancellation.Cancel();
            await Task.Delay(10);
            Assert.True(wait.IsCanceled);
            await Assert.ThrowsAsync<TaskCanceledException>(async() => await wait);
        }

        [Fact]
        public async Task ThrowsIfAlreadyInWaitState()
        {
            var handler = this.CreateHandler();
            handler.WaitAsync<AckMethod>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.WaitAsync<AckMethod>());
        }

        [Fact]
        public async Task IgnoresOtherMethods()
        {
            var handler = this.CreateHandler();
            var wait = handler.WaitAsync<AckMethod>();

            var handled = await ((IFrameHandler) handler).HandleMethodFrameAsync(MethodId.BasicDeliver, ReadOnlyMemory<byte>.Empty);

            Assert.False(handled);
            Assert.False(wait.IsCompleted);
        }

        [Fact]
        public async Task IgnoresContent()
        {
            var handler = this.CreateHandler();
            var wait = handler.WaitAsync<AckMethod>();

            var handled = await ((IFrameHandler) handler).HandleContentAsync(Substitute.For<IMessageProperties>(), ReadOnlySequence<byte>.Empty);

            Assert.False(handled);
            Assert.False(wait.IsCompleted);
        }

        [Fact]
        public async Task CanWait()
        {
            var handler = this.CreateHandler();
            var wait = handler.WaitAsync<AckMethod>();

            Assert.False(wait.IsCompleted);

            var handled = await ((IFrameHandler) handler).HandleMethodFrameAsync(MethodId.BasicAck, new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x18, 0x01 });
            await Task.Delay(10);

            Assert.True(handled);
            Assert.True(wait.IsCompleted);
            var data = (AckMethod) await wait;
            Assert.Equal((ulong)24, data.DeliveryTag);
            Assert.True(data.Multiple);
        }


        private WaitFrameHandler CreateHandler(Task channelCompletion = null)
        {
            var registry = new MethodRegistryBuilder().AddBasicMethods().Build();

            var ch = Substitute.For<IChannel>();
            channelCompletion ??= new TaskCompletionSource<bool>().Task;
            ch.Completion.Returns(channelCompletion);
            return new WaitFrameHandler(registry, ch);
        }
    }
}