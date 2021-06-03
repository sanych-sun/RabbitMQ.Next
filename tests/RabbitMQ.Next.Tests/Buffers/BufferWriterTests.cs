using System;
using System.Buffers;
using System.Collections.Generic;
using NSubstitute;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Buffers;
using Xunit;

namespace RabbitMQ.Next.Tests.Buffers
{
    public class BufferWriterTests
    {
        [Theory]
        [InlineData(-10)]
        [InlineData(200)]
        public void AdvanceThrowsOnInvalidArgument(int advance)
        {
            var bufferManager = this.MakeBufferManager();

            var bufferWriter = new BufferWriter(bufferManager);
            bufferWriter.GetMemory(100);

            Assert.Throws<ArgumentException>(() => bufferWriter.Advance(advance));
        }

        [Fact]
        public void ShouldExtendIfNeeded()
        {
            var bufferManager = this.MakeBufferManager();

            var bufferWriter = new BufferWriter(bufferManager);
            bufferWriter.GetMemory(100);
            bufferWriter.Advance(100);
            bufferWriter.GetMemory(100);
            bufferWriter.Advance(100);

            bufferManager.Received(2).Rent(Arg.Any<int>());
        }

        [Fact]
        public void DisposedWriterThrows()
        {
            var bufferManager = this.MakeBufferManager();

            var bufferWriter = new BufferWriter(bufferManager);
            bufferWriter.Dispose();

            Assert.Throws<ObjectDisposedException>(() => bufferWriter.GetSpan());
            Assert.Throws<ObjectDisposedException>(() => bufferWriter.GetMemory());
            Assert.Throws<ObjectDisposedException>(() => bufferWriter.Advance(1));
            Assert.Throws<ObjectDisposedException>(() => bufferWriter.ToSequence());
        }

        [Fact]
        public void CanDisposeMultiple()
        {
            var bufferManager = this.MakeBufferManager();

            var bufferWriter = new BufferWriter(bufferManager);
            bufferWriter.Dispose();

            var ex = Record.Exception(() => bufferWriter.Dispose());
            Assert.Null(ex);
        }


        [Fact]
        public void DisposeReleasesCreatedBuffers()
        {
            var bufferManager = this.MakeBufferManager(150);

            var bufferWriter = new BufferWriter(bufferManager);
            bufferWriter.GetMemory();
            bufferWriter.Advance(100);
            bufferWriter.GetMemory();
            bufferWriter.Advance(100);
            bufferWriter.GetMemory();
            bufferWriter.Advance(100);

            bufferWriter.Dispose();

            bufferManager.Received(3).Release(Arg.Any<byte[]>());
        }

        [Theory]
        [MemberData(nameof(BufferWriteTestCases))]
        public void CanWriteViaMemory(int dataLen, IEnumerable<int> chunkSizes)
        {
            var data = new byte[dataLen];
            var rnd = new Random();
            rnd.NextBytes(data);

            var bufferManager = this.MakeBufferManager();

            var bufferWriter = new BufferWriter(bufferManager);

            Span<byte> dataSpan = data;
            foreach (var chunk in chunkSizes)
            {
                var memory = bufferWriter.GetMemory(chunk);
                dataSpan.Slice(0, chunk).CopyTo(memory.Span);
                bufferWriter.Advance(chunk);

                dataSpan = dataSpan.Slice(chunk);
            }

            var written = bufferWriter.ToSequence().ToArray();
            Assert.Equal(data, written);
        }

        [Theory]
        [MemberData(nameof(BufferWriteTestCases))]
        public void CanWriteViaSpan(int dataLen, IEnumerable<int> chunkSizes)
        {
            var data = new byte[dataLen];
            var rnd = new Random();
            rnd.NextBytes(data);

            var bufferManager = this.MakeBufferManager();

            var bufferWriter = new BufferWriter(bufferManager);

            Span<byte> dataSpan = data;
            foreach (var chunk in chunkSizes)
            {
                var span = bufferWriter.GetSpan(chunk);
                dataSpan.Slice(0, chunk).CopyTo(span);
                bufferWriter.Advance(chunk);

                dataSpan = dataSpan.Slice(chunk);
            }

            var written = bufferWriter.ToSequence().ToArray();
            Assert.Equal(data, written);
        }

        public static IEnumerable<object[]> BufferWriteTestCases()
        {
            yield return new object[] { 10, new [] { 10 }};
            yield return new object[] { 10, new [] { 2, 5, 3 }};
            yield return new object[] { 200, new [] { 100, 100 }};
            yield return new object[] { 200, new [] { 200 }};
            yield return new object[] { 250, new [] { 150, 100 }};
            yield return new object[] { 300, new [] { 150, 150 }};
            yield return new object[] { 500, new [] { 100, 100, 100, 100, 100 }};
            yield return new object[] { 600, new [] { 100, 150, 100, 150, 100 }};
        }

        private IBufferManager MakeBufferManager(int bufferSize = 150)
        {
            var bufferManager = Substitute.For<IBufferManager>();
            bufferManager.Rent(Arg.Any<int>()).Returns(x =>
            {
                var requestedSize = x.Arg<int>();
                return (requestedSize < bufferSize) ? new byte[bufferSize] : new byte[requestedSize];
            });

            return bufferManager;
        }
    }
}