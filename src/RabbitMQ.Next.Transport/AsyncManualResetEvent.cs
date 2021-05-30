using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Transport
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

        public ValueTask WaitAsync(CancellationToken cancellationToken = default)
        {
            this.CheckDisposed();

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