using System;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Transport;
using Xunit;

namespace RabbitMQ.Next.Tests.Buffers
{
    public class BufferManagerTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(ProtocolConstants.FrameMinSize)]
        [InlineData(131072)]
        public void BufferSizeParameter(int bufferSize)
        {
            var bufferManager = new BufferManager(bufferSize);

            Assert.Equal(bufferSize, bufferManager.BufferSize);
        }

        [Fact]
        public void CanRentMoreThenConfiguredSize()
        {
            var bufferManager = new BufferManager(100);
            var buffer = bufferManager.Rent(200);

            Assert.Equal(200, buffer.Length);
        }

        [Theory]
        [InlineData(100, 0, 100)]
        [InlineData(100, 10, 100)]
        [InlineData(100, 200, 200)]
        [InlineData(100, ProtocolConstants.FrameMinSize, ProtocolConstants.FrameMinSize)]
        [InlineData(ProtocolConstants.FrameMinSize, 100, ProtocolConstants.FrameMinSize)]
        public void RentReturnsBufferCorrectSize(int bufferSize, int requestedSize, int expectedSize)
        {
            var bufferManager = new BufferManager(bufferSize);

            var buffer = bufferManager.Rent(requestedSize);

            Assert.Equal(expectedSize, buffer.Length);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public void WrongBufferSizeThrows(int bufferSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BufferManager(bufferSize));
        }

        [Theory]
        [InlineData(10, null)]
        [InlineData(10, 0)]
        [InlineData(10, 1)]
        [InlineData(10, 100)]
        public void ReturnIgnoresBufferWrongSize(int bufferSize, int? returnSize)
        {
            var bufferManager = new BufferManager(bufferSize);
            var buffer = returnSize.HasValue ? new byte[returnSize.Value] : null;
            bufferManager.Release(buffer);

            Assert.Equal(0, bufferManager.ReleasedItemsCount());
        }

        [Fact]
        public void ReturnStoreBuffer()
        {
            var bufferManager = new BufferManager(10);
            var buffer = bufferManager.Rent();

            bufferManager.Release(buffer);
            Assert.Equal(1, bufferManager.ReleasedItemsCount());
        }

        [Fact]
        public void RentReuseReleasedBuffers()
        {
            var bufferManager = new BufferManager(10);
            var buffer = bufferManager.Rent();

            bufferManager.Release(buffer);
            Assert.Equal(1, bufferManager.ReleasedItemsCount());

            var buffer2 = bufferManager.Rent();

            Assert.Equal(buffer, buffer2);
            Assert.Equal(0, bufferManager.ReleasedItemsCount());
        }

        [Fact]
        public void SetBufferSizeResetsReleased()
        {
            var bufferManager = new BufferManager(10);
            var buffer = bufferManager.Rent();

            bufferManager.Release(buffer);
            Assert.Equal(1, bufferManager.ReleasedItemsCount());

            bufferManager.SetBufferSize(20);
            Assert.Equal(0, bufferManager.ReleasedItemsCount());
        }

        [Fact]
        public void SetBufferSizeNotResetOnSameSize()
        {
            var bufferManager = new BufferManager(10);
            var buffer = bufferManager.Rent();

            bufferManager.Release(buffer);
            Assert.Equal(1, bufferManager.ReleasedItemsCount());

            bufferManager.SetBufferSize(10);
            Assert.Equal(1, bufferManager.ReleasedItemsCount());
        }

        [Theory]
        [InlineData(10, 5, 5)]
        [InlineData(1000, 500, 600)]
        public async Task ParallelTest(int threads, int minIterations, int maxIterations)
        {
            var bufferManager = new BufferManager(10);

            var tasks = new Task<byte[]>[threads];
            var rnd = new Random();
            for (var i = 0; i < threads; i++)
            {
                var iterations = rnd.Next(minIterations, maxIterations);
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    for (var i = 0; i < iterations; i++)
                    {
                        var buffer = bufferManager.Rent();
                        bufferManager.Release(buffer);
                    }

                    return bufferManager.Rent();
                });
            }

            await Task.WhenAll(tasks);

            var isUnique = tasks.Select(t => t.Result)
                .Distinct()
                .Count() == threads;

            Assert.True(isUnique);

        }
    }
}