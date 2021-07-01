using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.Transport;
using Xunit;

namespace RabbitMQ.Next.Tests.Channels
{
    public class SynchronizedChannelTests
    {
        [Fact]
        public async Task CanSendAsync()
        {
            var method = new DummyMethod<int>(MethodId.BasicGet, 12);
            ushort channelNumber = 1;

            var mock = this.CreateChannel(channelNumber);

            await mock.channel.SendAsync(method);

            await mock.frameSender.Received().SendMethodAsync(channelNumber, method);
        }

        [Fact]
        public async Task CanSendWithContentAsync()
        {
            var method = new DummyMethod<int>(MethodId.BasicGet, 12);
            var properties = new MessageProperties();
            var content = new ReadOnlySequence<byte>(new byte[] { 0x00, 0x01, 0x02, 0x03 });

            ushort channelNumber = 2;
            var mock = this.CreateChannel(channelNumber);

            await mock.channel.SendAsync(method, properties, content);

            Received.InOrder(async () =>
            {
                await mock.frameSender.SendMethodAsync(channelNumber, method);
                await mock.frameSender.SendContentHeaderAsync(channelNumber, properties, (ulong)content.Length);
                await mock.frameSender.SendContentAsync(channelNumber, content);
            });
        }

        [Fact]
        public async Task CanSendWithEmptyContentAsync()
        {
            var method = new DummyMethod<int>(MethodId.BasicGet, 12);
            var properties = new MessageProperties();
            var content = ReadOnlySequence<byte>.Empty;

            ushort channelNumber = 3;
            var mock = this.CreateChannel(channelNumber);

            await mock.channel.SendAsync(method, properties, content);

            Received.InOrder(async () =>
            {
                await mock.frameSender.SendMethodAsync(channelNumber, method);
                await mock.frameSender.SendContentHeaderAsync(channelNumber, properties, (ulong)content.Length);
            });
        }

        [Fact]
        public async Task CanWaitAsync()
        {
            ushort channelNumber = 8;
            var mock = this.CreateChannel(channelNumber);

            var wait = mock.channel.WaitAsync<DummyMethod<int>>();
            Assert.False(wait.IsCompleted);

            var message = new DummyMethod<int>(MethodId.BasicGetEmpty, 123);
            await mock.waitHandler.HandleAsync(message, null, ReadOnlySequence<byte>.Empty);

            Assert.Equal(message, await wait);
        }

        [Fact]
        public async Task CanCancelWaitAsync()
        {
            ushort channelNumber = 12;
            var cancellation = new CancellationTokenSource();
            var mock = this.CreateChannel(channelNumber);

            var wait = mock.channel.WaitAsync<DummyMethod<int>>(cancellation.Token);
            Assert.False(wait.IsCompleted);

            cancellation.Cancel();

            await Assert.ThrowsAsync<TaskCanceledException>(async () => await wait);
        }

        private (SynchronizedChannel channel, IFrameSender frameSender, IMethodHandler waitHandler) CreateChannel(ushort channelNumber)
        {
            var registry = Substitute.For<IMethodRegistry>();
            registry.GetMethodId<DummyMethod<int>>().Returns(MethodId.BasicGetEmpty);
            var ch = Substitute.For<IChannel>();
            var tcs = new TaskCompletionSource<bool>();
            ch.Completion.Returns(tcs.Task);
            var waitHandler = new WaitMethodHandler(registry, ch);

            var frameSender = Substitute.For<IFrameSender>();

            var channel = new SynchronizedChannel(channelNumber, frameSender, waitHandler);

            return (channel, frameSender, waitHandler);
        }
    }
}