using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Tasks
{
    public class AsyncManualResetEvent : IDisposable
    {
        private volatile TaskCompletionSource<bool> completionSource;
        private volatile bool disposed;

        public AsyncManualResetEvent(bool initialState = false)
        {
            if (!initialState)
            {
                this.completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }

        public void Set()
        {
            this.CheckDisposed();
            this.completionSource?.TrySetResult(true);
        }

        public ValueTask<bool> WaitAsync(int milliseconds = 0, CancellationToken cancellation = default)
        {
            this.CheckDisposed();

            if (this.completionSource == null)
            {
                return new ValueTask<bool>(true);
            }

            if (this.completionSource.Task.IsCompleted)
            {
                return new ValueTask<bool>(true);
            }

            var innerTask = this.completionSource.Task;


            if (milliseconds > 0)
            {
                var delayTask = Task.Delay(milliseconds);
                innerTask = Task.WhenAny(innerTask, delayTask)
                    .ContinueWith(t => t.Result != delayTask);
            }

            if (cancellation.CanBeCanceled)
            {
                innerTask = innerTask.WithCancellation(cancellation);
            }

            return new ValueTask<bool>(innerTask);
        }

        public void Reset()
        {
            this.CheckDisposed();
            var currentCompletionSource = this.completionSource;

            if (currentCompletionSource != null && !currentCompletionSource.Task.IsCompleted)
            {
                return;
            }

            Interlocked.CompareExchange(ref this.completionSource, new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously), currentCompletionSource);
        }

        public void Dispose()
        {
            this.disposed = true;
            var currentCompletionSource = this.completionSource;
            this.completionSource = null;

            currentCompletionSource?.SetCanceled();
        }

        private void CheckDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(AsyncManualResetEvent));
            }
        }
    }
}