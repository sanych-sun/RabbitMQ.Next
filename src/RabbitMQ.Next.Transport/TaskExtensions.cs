using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Transport
{
    public static class TaskExtensions
    {
        public static async Task WithCancellation(this Task task, CancellationToken token)
        {
            var cancellationSource = new TaskCompletionSource<bool>();
            await using var registration = token.Register(tcs => ((TaskCompletionSource<bool>)tcs).TrySetCanceled(), cancellationSource);

            await Task.WhenAny(task, cancellationSource.Task);
            token.ThrowIfCancellationRequested();
        }

        public static async Task<TResult> WithCancellation<TResult>(this Task<TResult> task, CancellationToken token)
        {
            var cancellationSource = new TaskCompletionSource<bool>();
            await using var registration = token.Register(tcs => ((TaskCompletionSource<bool>)tcs).TrySetCanceled(), cancellationSource);

            await Task.WhenAny(task, cancellationSource.Task);

            token.ThrowIfCancellationRequested();
            return await task;
        }
    }
}