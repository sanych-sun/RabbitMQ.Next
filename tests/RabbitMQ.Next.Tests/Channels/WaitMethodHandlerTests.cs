using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Channels;
using Xunit;

namespace RabbitMQ.Next.Tests.Channels
{
    public class WaitMethodHandlerTests
    {
        [Fact]
        public void DisposeMultiple()
        {
            var handler = this.CreateHandler();

            handler.Dispose();
            var ex = Record.Exception(() => handler.Dispose());

            Assert.Null(ex);
        }

        [Fact]
        public void DisposeCancelsWait()
        {
            var handler = this.CreateHandler();

            var wait = handler.WaitAsync<DummyMethod<int>>();
            Assert.False(wait.IsCanceled);

            handler.Dispose();
            Assert.True(wait.IsCanceled);
        }

        [Fact]
        public void CancelsOnCancellationToken()
        {
            var handler = this.CreateHandler();
            var cancellation = new CancellationTokenSource();

            var wait = handler.WaitAsync<DummyMethod<int>>(cancellation.Token);
            Assert.False(wait.IsCanceled);

            cancellation.Cancel();
            Assert.True(wait.IsCanceled);
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

            Assert.True(handled);
            Assert.True(wait.IsCompleted);
            var data = (DummyMethod<int>) await wait;
            Assert.Equal(42, data.Data);
        }


        private WaitMethodHandler CreateHandler()
        {
            var registry = Substitute.For<IMethodRegistry>();
            registry.GetMethodId<DummyMethod<int>>().Returns(MethodId.BasicGetEmpty);

            return new WaitMethodHandler(registry);
        }
    }
}