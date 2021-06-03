using System;
using NSubstitute;
using RabbitMQ.Next.Abstractions.Buffers;
using Xunit;

namespace RabbitMQ.Next.Tests.Buffers
{
    public class MemoryOwnerTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        public void MemoryRentsBuffer(int size)
        {
            var bufferManager = Substitute.For<IBufferManager>();

            new MemoryOwner(bufferManager, size);

            bufferManager.Received().Rent(size);
        }

        [Fact]
        public void ThrowsOnWrongSize()
        {
            var bufferManager = Substitute.For<IBufferManager>();

            Assert.Throws<ArgumentOutOfRangeException>(() => new MemoryOwner(bufferManager, -5));
        }

        [Fact]
        public void MemoryReturnsSameBuffer()
        {
            var bufferManager = Substitute.For<IBufferManager>();
            bufferManager.Rent(Arg.Any<int>()).Returns(new byte[10]);

            var memoryOwner = new MemoryOwner(bufferManager, 10);
            var memory = memoryOwner.Memory;
            var memory2 = memoryOwner.Memory;

            Assert.Equal(memory, memory2);
        }

        [Fact]
        public void DisposeReturnsBuffer()
        {
            var buffer = new byte[10];

            var bufferManager = Substitute.For<IBufferManager>();
            bufferManager.Rent(Arg.Any<int>()).Returns(buffer);

            var memoryOwner = new MemoryOwner(bufferManager, 10);

            memoryOwner.Dispose();

            bufferManager.Received().Release(buffer);
        }

        [Fact]
        public void DisposedThrows()
        {
            var bufferManager = Substitute.For<IBufferManager>();

            var memoryOwner = new MemoryOwner(bufferManager, 10);
            memoryOwner.Dispose();

            Assert.Throws<ObjectDisposedException>(() => memoryOwner.Memory);
        }

        [Fact]
        public void CanDisposeMultiple()
        {
            var bufferManager = Substitute.For<IBufferManager>();

            var memoryOwner = new MemoryOwner(bufferManager, 10);

            memoryOwner.Dispose();

            var exception = Record.Exception(() => memoryOwner.Dispose());
            Assert.Null(exception);
        }
    }
}