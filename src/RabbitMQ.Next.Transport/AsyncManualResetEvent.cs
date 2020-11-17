using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Transport
{
    public class AsyncManualResetEvent
    {
        private volatile TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();

        public AsyncManualResetEvent(bool initialState = false)
        {
            if (initialState)
            {
                this.completionSource.SetResult(true);
            }
        }

        public void Set()
        {
            this.completionSource.TrySetResult(true);
        }

        public Task WaitAsync(CancellationToken cancellationToken = default)
        {
            if (!cancellationToken.CanBeCanceled)
            {
                return this.completionSource.Task;
            }

            return this.completionSource.Task.WithCancellation(cancellationToken);
        }

        public void Reset()
        {
            var currentCompletionSource = this.completionSource;

            if (!currentCompletionSource.Task.IsCompleted)
            {
                return;
            }

            Interlocked.CompareExchange(ref this.completionSource, new TaskCompletionSource<bool>(), currentCompletionSource);
        }
    }
}