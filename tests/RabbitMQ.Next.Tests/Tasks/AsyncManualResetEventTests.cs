using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Tasks;
using Xunit;

namespace RabbitMQ.Next.Tests.Tasks
{
    public class AsyncManualResetEventTests
    {
        [Fact]
        public async Task DisposeShouldCancelWait()
        {
            var evt = new AsyncManualResetEvent();
            var wait = evt.WaitAsync();

            await Task.Yield();

            evt.Dispose();
            await Task.Yield();

            Assert.True(wait.IsCanceled);
            await Assert.ThrowsAsync<TaskCanceledException>(async () => await wait);
        }

        [Fact]
        public async Task DisposedThrowsOnWait()
        {
            var evt = new AsyncManualResetEvent();
            evt.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await evt.WaitAsync());
        }

        [Fact]
        public void DisposedThrowsOnSet()
        {
            var evt = new AsyncManualResetEvent();
            evt.Dispose();
            Assert.Throws<ObjectDisposedException>(() => evt.Set());
        }

        [Fact]
        public void DisposedThrowsOnReset()
        {
            var evt = new AsyncManualResetEvent();
            evt.Dispose();
            Assert.Throws<ObjectDisposedException>(() => evt.Reset());
        }

        [Fact]
        public void MultipleDispose()
        {
            var evt = new AsyncManualResetEvent();

            evt.Dispose();
            var ex = Record.Exception(() => evt.Dispose());
            Assert.Null(ex);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldAcceptDefaultState(bool isSet)
        {
            var evt = new AsyncManualResetEvent(isSet);

            var wait = evt.WaitAsync();

            Assert.Equal(isSet, wait.IsCompleted);
        }

        [Fact]
        public async Task SetShouldCompleteWait()
        {
            var evt = new AsyncManualResetEvent();

            var wait = evt.WaitAsync();
            Assert.False(wait.IsCompleted);

            evt.Set();
            Assert.True(await wait);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanSetMultiple(bool initialState)
        {
            var evt = new AsyncManualResetEvent(initialState);

            evt.Set();

            var ex = Record.Exception(() => evt.Set());
            Assert.Null(ex);
        }

        [Fact]
        public async Task AlreadySetReturnCompletedTask()
        {
            var evt = new AsyncManualResetEvent();
            evt.Set();

            var wait = evt.WaitAsync();

            Assert.True(wait.IsCompleted);
            Assert.True(await wait);
        }

        [Fact]
        public void Reset()
        {
            var evt = new AsyncManualResetEvent();
            evt.Set();

            Assert.True(evt.WaitAsync().IsCompleted);

            evt.Reset();
            var wait = evt.WaitAsync();

            Assert.False(wait.IsCompleted);
        }

        [Fact]
        public void CanResetMultiple()
        {
            var evt = new AsyncManualResetEvent();

            evt.Reset();
            var ex = Record.Exception(() => evt.Reset());
            Assert.Null(ex);
        }

        [Fact]
        public async Task SetAfterReset()
        {
            var evt = new AsyncManualResetEvent();
            evt.Set();

            Assert.True(evt.WaitAsync().IsCompleted);

            evt.Reset();
            var wait = evt.WaitAsync();
            evt.Set();

            await Task.Yield();

            Assert.True(wait.IsCompleted);
            Assert.True(await wait);
        }

        [Fact]
        public async Task WaitWithTimeout()
        {
            var evt = new AsyncManualResetEvent();

            var wait = evt.WaitAsync(10);
            await Task.Delay(20);

            Assert.True(wait.IsCompleted);
            Assert.False(await wait);
        }

        [Fact]
        public async Task WaitWithCancellation()
        {
            var evt = new AsyncManualResetEvent();
            var cs = new CancellationTokenSource();

            var wait = evt.WaitAsync(0, cs.Token);
            Assert.False(wait.IsCompleted);
            cs.Cancel();
            await Task.Yield();

            await Assert.ThrowsAsync<TaskCanceledException>(async () => await wait);
        }

        [Fact]
        public async Task WaitWithTimeoutAndCancellation()
        {
            var evt = new AsyncManualResetEvent();
            var cs = new CancellationTokenSource();

            var wait = evt.WaitAsync(100, cs.Token);
            Assert.False(wait.IsCompleted);
            cs.Cancel();
            await Task.Yield();

            await Assert.ThrowsAsync<TaskCanceledException>(async () => await wait);
        }
    }
}