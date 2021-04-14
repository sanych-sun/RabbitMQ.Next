using System;
using NSubstitute;
using RabbitMQ.Next.Abstractions.Buffers;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Buffers
{
    public class MemoryOwnerTests
    {
        [Fact]
        public void MemoryRentsBuffer()
        {
            var bufferManager = Substitute.For<IBufferManager>();
            bufferManager.Rent(Arg.Any<int>()).Returns(new byte[10]);

            var memoryOwner = new MemoryOwner(bufferManager, 10);
            var memory = memoryOwner.Memory;

            bufferManager.Received().Rent(10);
            Assert.Equal(10, memory.Length);
        }

        [Fact]
        public void EmptySizeDoesNotRentBuffer()
        {
            var bufferManager = Substitute.For<IBufferManager>();

            var memoryOwner = new MemoryOwner(bufferManager, 0);
            var memory = memoryOwner.Memory;

            bufferManager.DidNotReceive().Rent(Arg.Any<int>());
            Assert.Equal(0, memory.Length);
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