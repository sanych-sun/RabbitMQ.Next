using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Tasks;
using Xunit;

namespace RabbitMQ.Next.Tests.Tasks
{
    public class TaskExtensionsTests
    {
        [Fact]
        public async Task WithCancellationAlreadyCompleted()
        {
            var task = Task.FromResult(2);
            var cancellation = new CancellationTokenSource();

            var wrapped = task.WithCancellation(cancellation.Token);
            await Task.Yield();

            Assert.True(wrapped.IsCompleted);
            Assert.Equal(2, await wrapped);
            Assert.Equal(task, wrapped);
        }

        [Fact]
        public async Task WithCancellationDefault()
        {
            var tcs = new TaskCompletionSource<int>();
            var task = tcs.Task;

            var wrapped = task.WithCancellation(default);
            await Task.Yield();
            tcs.SetResult(5);

            Assert.True(wrapped.IsCompleted);
            Assert.Equal(5, await wrapped);
            Assert.Equal(task, wrapped);
        }

        [Fact]
        public async Task WithCancellationAlreadyCancelled()
        {
            var tcs = new TaskCompletionSource<int>();
            var task = tcs.Task;
            tcs.SetCanceled();
            var cancellation = new CancellationTokenSource();

            var wrapped = task.WithCancellation(cancellation.Token);
            await Task.Yield();

            Assert.True(wrapped.IsCompleted);
            await Assert.ThrowsAsync<TaskCanceledException>(async() => await wrapped);
        }

        [Fact]
        public async Task WithCancellationReturnsResult()
        {
            var tcs = new TaskCompletionSource<int>();
            var task = tcs.Task;
            var cancellation = new CancellationTokenSource();

            var wrapped = task.WithCancellation(cancellation.Token);
            tcs.SetResult(5);
            await Task.Yield();

            Assert.True(wrapped.IsCompleted);
            Assert.Equal(5, await wrapped);
        }

        [Fact]
        public async Task WithCancellationThrowsOnCancel()
        {
            var tcs = new TaskCompletionSource<int>();
            var task = tcs.Task;
            var cancellation = new CancellationTokenSource();

            var wrapped = task.WithCancellation(cancellation.Token);
            cancellation.Cancel();
            await Task.Yield();

            Assert.True(wrapped.IsCanceled);
            await Assert.ThrowsAsync<TaskCanceledException>(async() => await wrapped);
        }
    }
}