using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Tests.Mocks;
using Xunit;

namespace RabbitMQ.Next.Tests.Channels
{
    public class WaitMethodHandlerTests
    {
        [Fact]
        public async Task WaitThrowsOnCompletedChannel()
        {
            var handler = this.CreateHandler(Task.CompletedTask);

            await Assert.ThrowsAsync<InvalidOperationException>(async() => await handler.WaitAsync<DummyMethod<int>>());
        }

        [Fact]
        public async Task WaitThrowsOnChannelError()
        {
            var tcs = new TaskCompletionSource<bool>();
            var handler = this.CreateHandler(tcs.Task);

            var wait = handler.WaitAsync<DummyMethod<int>>();
            var ex = new ArgumentNullException();
            tcs.SetException(ex);

            await Assert.ThrowsAsync<ArgumentNullException>(async() => await wait);
        }

        [Fact]
        public async Task WaitCancelsOnChannelCompletion()
        {
            var tcs = new TaskCompletionSource<bool>();
            var handler = this.CreateHandler(tcs.Task);

            var wait = handler.WaitAsync<DummyMethod<int>>();
            tcs.SetResult(true);

            await Assert.ThrowsAsync<TaskCanceledException>(async() => await wait);
        }

        [Fact]
        public async Task CancelsOnCancellationToken()
        {
            var handler = this.CreateHandler();
            var cancellation = new CancellationTokenSource();

            var wait = handler.WaitAsync<DummyMethod<int>>(cancellation.Token);
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
            handler.WaitAsync<DummyMethod<int>>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.WaitAsync<DummyMethod<int>>());
        }

        [Fact]
        public async Task IgnoresOtherMethods()
        {
            var handler = this.CreateHandler();
            var wait = handler.WaitAsync<DummyMethod<int>>();

            var handled = await ((IMethodHandler) handler).HandleAsync(new DummyMethod<string>(MethodId.BasicDeliver, "test"), null, ReadOnlySequence<byte>.Empty);

            Assert.False(handled);
            Assert.False(wait.IsCompleted);
        }

        [Fact]
        public async Task CanWait()
        {
            var handler = this.CreateHandler();
            var wait = handler.WaitAsync<DummyMethod<int>>();

            Assert.False(wait.IsCompleted);

            var handled = await ((IMethodHandler) handler).HandleAsync(new DummyMethod<int>(MethodId.BasicGetEmpty, 42), null, ReadOnlySequence<byte>.Empty);
            await Task.Delay(10);

            Assert.True(handled);
            Assert.True(wait.IsCompleted);
            var data = (DummyMethod<int>) await wait;
            Assert.Equal(42, data.Data);
        }


        private WaitMethodHandler CreateHandler(Task channelCompletion = null)
        {
            var registry = Substitute.For<IMethodRegistry>();
            registry.GetMethodId<DummyMethod<int>>().Returns(MethodId.BasicGetEmpty);

            var ch = Substitute.For<IChannel>();
            channelCompletion ??= new TaskCompletionSource<bool>().Task;
            ch.Completion.Returns(channelCompletion);
            return new WaitMethodHandler(registry, ch);
        }
    }
}