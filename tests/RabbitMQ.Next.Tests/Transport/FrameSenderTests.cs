using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Sockets;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.Transport;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport
{
    public class FrameSenderTests
    {
        [Fact]
        public void ThrowsOnNullSocket()
        {
            var bufferPool = Substitute.For<IBufferPool>();
            var registry = Substitute.For<IMethodRegistry>();

            Assert.Throws<ArgumentNullException>(() => new FrameSender(null, registry, bufferPool));
        }

        [Fact]
        public void ThrowsOnNullRegistry()
        {
            var socket = Substitute.For<ISocket>();
            var bufferPool = Substitute.For<IBufferPool>();

            Assert.Throws<ArgumentNullException>(() => new FrameSender(socket, null, bufferPool));
        }

        [Fact]
        public void ThrowsOnNullBufferPool()
        {
            var socket = Substitute.For<ISocket>();
            var registry = Substitute.For<IMethodRegistry>();

            Assert.Throws<ArgumentNullException>(() => new FrameSender(socket, registry, null));
        }

        [Fact]
        public void CanDisposeMultiple()
        {
            var mock = CreateFrameSender();

            mock.Sender.Dispose();

            var ex = Record.Exception(() => mock.Sender.Dispose());
            Assert.Null(ex);
        }

        [Fact]
        public async Task SendAmqpHeaderAsync()
        {
            var mock = CreateFrameSender();

            await mock.Sender.SendAmqpHeaderAsync();

            Assert.Equal(new byte[] { 0x41, 0x4D, 0x51, 0x50, 0x00, 0x00, 0x09, 0x01 }, mock.GetWrittenBytes().ToArray());
        }

        [Fact]
        public async Task SendHeartBeatAsync()
        {
            var mock = CreateFrameSender();

            await mock.Sender.SendHeartBeatAsync();

            Assert.Equal(new byte[] { 0x08, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xCE }, mock.GetWrittenBytes().ToArray());
        }

        [Fact]
        public async Task SendMethodAsync()
        {
            var mock = CreateFrameSender();

            await mock.Sender.SendMethodAsync(5, new DummyMethod<int>(MethodId.BasicGet, 0));

            Assert.Equal(
                new byte[]
                {
                    // frame header
                    0x01, // method frame
                    0x00, 0x05, // channel number
                    0x00, 0x00, 0x00, 0x06, // payload size
                    // payload
                    0x00, 0x3C, 0x00, 0x46, // methodId
                    0x01, 0x02, // method args
                    // frame-end
                    0xCE
                },
                mock.GetWrittenBytes().ToArray());
        }

        [Fact]
        public async Task SendContentHeaderAsync()
        {
            var mock = CreateFrameSender();

            await mock.Sender.SendContentHeaderAsync(5, new MessagePropertiesMock { ContentType = "json"}, 42);

            Assert.Equal(
                new byte[]
                {
                    // frame header
                    0x02, // content header frame
                    0x00, 0x05, // channel number
                    0x00, 0x00, 0x00, 0x13, // payload size
                    // payload
                    0, 60, 0, 0, 0, 0, 0, 0, 0, 0, 0, 42, 0b_10000000, 0b_00000000, 0x04, 0x6A, 0x73, 0x6F, 0x6E,
                    // frame-end
                    0xCE
                },
                mock.GetWrittenBytes().ToArray());
        }

        [Theory]
        [MemberData(nameof(SendContentAsyncTestCases))]
        public async Task SendContentAsync(byte[] expected, ushort channel, ReadOnlySequence<byte> content)
        {
            var mock = CreateFrameSender();

            await mock.Sender.SendContentAsync(channel, content);

            Assert.Equal(expected, mock.GetWrittenBytes().ToArray());
        }

        public static IEnumerable<object[]> SendContentAsyncTestCases()
        {
            yield return new object[]
            {
                new byte[]
                {
                    // frame header
                    0x03, // content frame
                    0x00, 0x02, // channel number
                    0x00, 0x00, 0x00, 0x03, // payload size
                    // payload
                    0x01, 0x02, 0x03,
                    // frame-end
                    0xCE
                },
                2, // channel
                Helpers.MakeSequence(new byte[] {0x01, 0x02, 0x03})
            };

            yield return new object[]
            {
                new byte[]
                {
                    // frame header
                    0x03, // content frame
                    0x00, 0x02, // channel number
                    0x00, 0x00, 0x00, 0x03, // payload size
                    // payload
                    0x01, 0x02, 0x03,
                    // frame-end
                    0xCE,

                    // frame header
                    0x03, // content frame
                    0x00, 0x02, // channel number
                    0x00, 0x00, 0x00, 0x02, // payload size
                    // payload
                    0x04, 0x05,
                    // frame-end
                    0xCE,
                },
                2, // channel
                Helpers.MakeSequence(new byte[] {0x01, 0x02, 0x03 }, new byte[] {0x04, 0x05 })
            };

            yield return new object[]
            {
                new byte[]
                {
                    // frame header
                    0x03, // content frame
                    0x00, 0x02, // channel number
                    0x00, 0x00, 0x00, 0x19, // payload size
                    // payload
                    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19,
                    // frame-end
                    0xCE,

                    // frame header
                    0x03, // content frame
                    0x00, 0x02, // channel number
                    0x00, 0x00, 0x00, 0x06, // payload size
                    // payload
                    0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
                    // frame-end
                    0xCE,
                },
                2, // channel
                Helpers.MakeSequence(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F })
            };
        }

        private static (Func<ReadOnlyMemory<byte>> GetWrittenBytes, FrameSender Sender) CreateFrameSender()
        {
            var socket = new SocketMock(10_000);

            var dummyFormatter = new DummyFormatter<DummyMethod<int>>(new byte[] {0x01, 0x02});
            var registry = Substitute.For<IMethodRegistry>();
            registry.GetFormatter<DummyMethod<int>>().Returns(dummyFormatter);

            var bufferPool = Substitute.For<IBufferPool>();
            var bufferManager = Substitute.For<IBufferManager>();
            bufferManager.Rent(Arg.Any<int>()).ReturnsForAnyArgs(x => new byte[1024]);

            bufferPool.CreateMemory(Arg.Any<int>()).Returns(call => new MemoryOwner(bufferManager, (int) call[0]));

            var sender = new FrameSender(socket, registry, bufferPool);
            sender.FrameMaxSize = 25;
            return (socket.GetWrittenBytes, sender);
        }
    }
}