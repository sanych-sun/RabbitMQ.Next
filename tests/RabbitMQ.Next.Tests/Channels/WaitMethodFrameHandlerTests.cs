using System;
using System.Buffers;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Basic;
using RabbitMQ.Next.Transport.Methods.Registry;
using Xunit;

namespace RabbitMQ.Next.Tests.Channels
{
    public class WaitMethodFrameHandlerTests
    {
        [Fact]
        public async Task WaitCancelsOnReset()
        {
            var handler = this.CreateHandler<AckMethod>();

            var wait = handler.WaitTask;
            handler.Release();

            await Assert.ThrowsAsync<TaskCanceledException>(async() => await wait);
        }

        [Fact]
        public void IgnoresOtherMethods()
        {
            var handler = this.CreateHandler<AckMethod>();
            var wait = handler.WaitTask;

            var handled = handler.HandleMethodFrame(MethodId.BasicDeliver, ReadOnlySpan<byte>.Empty);

            Assert.False(handled);
            Assert.False(wait.IsCompleted);
        }

        [Fact]
        public async Task IgnoresContent()
        {
            var handler = this.CreateHandler<AckMethod>();
            var wait = handler.WaitTask;

            var handled = await handler.HandleContentAsync(Substitute.For<IMessageProperties>(), ReadOnlySequence<byte>.Empty);

            Assert.False(handled);
            Assert.False(wait.IsCompleted);
        }

        [Fact]
        public async Task CanWait()
        {
            var handler = this.CreateHandler<AckMethod>();
            var wait = handler.WaitTask;

            Assert.False(wait.IsCompleted);

            var handled = ((IFrameHandler) handler).HandleMethodFrame(MethodId.BasicAck, new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x18, 0x01 });
            await Task.Delay(10);

            Assert.True(handled);
            Assert.True(wait.IsCompleted);
            var data = await wait;
            Assert.Equal((ulong)24, data.DeliveryTag);
            Assert.True(data.Multiple);
        }


        private WaitMethodFrameHandler<TMethod> CreateHandler<TMethod>()
            where TMethod: struct, IIncomingMethod
        {
            var registry = new MethodRegistryBuilder().AddBasicMethods().Build();

            return new WaitMethodFrameHandler<TMethod>(registry);
        }
    }
}