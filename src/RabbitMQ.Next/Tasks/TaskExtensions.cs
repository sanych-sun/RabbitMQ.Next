using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Tasks
{
    public static class TaskExtensions
    {
        public static Task AsTask(this CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            var tcs = new TaskCompletionSource();
            cancellation.Register(s => ((TaskCompletionSource)s).TrySetResult(), tcs);
            return tcs.Task;
        }

        public static Task<TResult> WithCancellation<TResult>(this Task<TResult> task, CancellationToken cancellation)
        {
            if (task.IsCompleted)
            {
                return task;
            }

            if (cancellation.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }

            if (!cancellation.CanBeCanceled)
            {
                return task;
            }

            return task.WrapTask(cancellation);
        }

        public static CancellationToken Combine(this CancellationToken token, CancellationToken other)
        {
            if (token.IsCancellationRequested || other.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }

            if (!token.CanBeCanceled)
            {
                return other;
            }

            if (!other.CanBeCanceled)
            {
                return token;
            }

            return CancellationTokenSource.CreateLinkedTokenSource(token, other).Token;
        }

        private static async Task<TResult> WrapTask<TResult>(this Task<TResult> task, CancellationToken cancellation)
        {
            var cancellationSource = new TaskCompletionSource<bool>();
            await using var registration = cancellation.Register(tcs => ((TaskCompletionSource<bool>)tcs).TrySetResult(true), cancellationSource);

            await Task.WhenAny(task, cancellationSource.Task);

            if (cancellation.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }

            return await task;
        }
    }
}