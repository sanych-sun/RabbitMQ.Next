using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Transport;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport
{
    public class PipeExtensionsTests
    {
        [Fact]
        public async Task ReadThrowsOnCancellation()
        {
            var pipe = new Pipe();

            var cancellationSource = new CancellationTokenSource();
            var readerTask = pipe.Reader.ReadAsync(1, _ => true, cancellationSource.Token);

            await Task.Delay(10);

            cancellationSource.Cancel();

            Assert.Throws<OperationCanceledException>(() => readerTask.GetAwaiter().GetResult());
        }

        [Theory]
        [InlineData(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 0)]
        [InlineData(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 2)]
        [InlineData(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 10)]
        public async Task ReadAsync(byte[] payload, int length)
        {
            var expected = new byte[length];
            Array.Copy(payload, 0, expected, 0, length);
            var pipe = new Pipe();

            var readerTask = pipe.Reader.ReadAsync(length, data => data.ToArray());

            await Task.Delay(10);
            await pipe.Writer.WriteAsync(payload);

            var result = await readerTask;

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ReadAsyncShouldWait()
        {
            var pipe = new Pipe();

            var readTask = pipe.Reader.ReadAsync(5, data => data.ToArray());

            await Task.Delay(10);
            await pipe.Writer.WriteAsync(new byte[] {1, 2, 3});

            await Task.Delay(10);
            Assert.False(readTask.IsCompleted);

            await pipe.Writer.WriteAsync(new byte[] {4, 5, 6});
            await Task.Delay(100);
            Assert.True(readTask.IsCompleted);
            Assert.Equal(new byte[] {1, 2, 3, 4, 5}, readTask.GetAwaiter().GetResult());
        }

        [Fact]
        public async Task ReadAsyncShouldExitOnComplete()
        {
            var pipe = new Pipe();

            var readTask = pipe.Reader.ReadAsync(5, _ => true);

            await pipe.Writer.WriteAsync(new byte[] {1, 2});
            await pipe.Writer.CompleteAsync();

            await Task.Delay(10);

            Assert.False(readTask.GetAwaiter().GetResult());
        }
    }
}