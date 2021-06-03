using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Tasks
{
    public static class TaskExtensions
    {
        public static Task<TResult> WithCancellation<TResult>(this Task<TResult> task, CancellationToken token)
        {
            if (task.IsCompleted)
            {
                return task;
            }

            if (!token.CanBeCanceled)
            {
                return task;
            }

            return task.WrapTask(token);
        }

        private static async Task<TResult> WrapTask<TResult>(this Task<TResult> task, CancellationToken token)
        {
            var cancellationSource = new TaskCompletionSource<bool>();
            await using var registration = token.Register(tcs => ((TaskCompletionSource<bool>)tcs).TrySetCanceled(), cancellationSource);

            await Task.WhenAny(task, cancellationSource.Task);

            if (token.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }

            return await task;
        }
    }
}