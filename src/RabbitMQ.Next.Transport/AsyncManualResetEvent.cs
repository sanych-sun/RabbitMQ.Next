using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Transport
{
    public class AsyncManualResetEvent
    {
        private volatile TaskCompletionSource<bool> completionSource;

        public AsyncManualResetEvent(bool initialState = false)
        {
            if (!initialState)
            {
                this.completionSource = new TaskCompletionSource<bool>();
            }
        }

        public void Set()
        {
            this.completionSource?.TrySetResult(true);
        }

        public ValueTask WaitAsync(CancellationToken cancellationToken = default)
        {
            if (this.completionSource == null)
            {
                return default;
            }

            if (this.completionSource.Task.IsCompleted)
            {
                return default;
            }

            if (!cancellationToken.CanBeCanceled)
            {
                return new ValueTask(this.completionSource.Task);
            }

            return new ValueTask(this.completionSource.Task.WithCancellation(cancellationToken));
        }

        public void Reset()
        {
            var currentCompletionSource = this.completionSource;

            if (currentCompletionSource != null && !currentCompletionSource.Task.IsCompleted)
            {
                return;
            }

            Interlocked.CompareExchange(ref this.completionSource, new TaskCompletionSource<bool>(), currentCompletionSource);
        }
    }
}