using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer.Abstractions.Acknowledger
{
    internal class MultipleMessageAcknowledger : IAcknowledger
    {
        private readonly IAcknowledgement acknowledgement;
        private readonly CancellationTokenSource cts;
        private readonly int pendingLimit;
        private readonly int timeoutMs;
        private long lastProcessed;
        private int pendingCount;
        private Task pendingSendTask;

        public MultipleMessageAcknowledger(IAcknowledgement acknowledgement, TimeSpan timeout, int count)
        {
            if (acknowledgement == null)
            {
                throw new ArgumentNullException(nameof(acknowledgement));
            }

            if(timeout <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout));
            }

            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            this.acknowledgement = acknowledgement;
            this.cts = new CancellationTokenSource();
            this.pendingLimit = count;
            this.timeoutMs = (int)timeout.TotalMilliseconds;
        }

        public async ValueTask AckAsync(ulong deliveryTag)
        {
            this.CheckDisposed();

            var count = Interlocked.Increment(ref this.pendingCount);
            var prev = Interlocked.Exchange(ref this.lastProcessed, (long)deliveryTag);

            if (count >= this.pendingLimit)
            {
                await this.SendPendingAck();
            }
            else if (prev == 0)
            {
                var pendingTask = this.pendingSendTask;
                if (pendingTask == null || pendingTask.IsCompleted)
                {
                    this.pendingSendTask = Task.Run(async () =>
                    {
                        await Task.Delay(this.timeoutMs, this.cts.Token)
                            .ContinueWith(c => { }); // use empty continuation to suppress the TaskCancelledException
                        if (!this.cts.Token.IsCancellationRequested)
                        {
                            await this.SendPendingAck();
                        }
                    });
                }
            }
        }

        public async ValueTask NackAsync(ulong deliveryTag, bool requeue)
        {
            this.CheckDisposed();

            await this.SendPendingAck();
            await this.acknowledgement.NackAsync(deliveryTag, requeue);
        }

        public async ValueTask DisposeAsync()
        {
            if (this.cts.IsCancellationRequested)
            {
                return;
            }

            this.cts.Cancel();

            await this.SendPendingAck();
            await this.acknowledgement.DisposeAsync();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed()
        {
            if (this.cts.IsCancellationRequested)
            {
                throw new ObjectDisposedException(nameof(MultipleMessageAcknowledger));
            }
        }

        private async ValueTask SendPendingAck()
        {
            Interlocked.Exchange(ref this.pendingCount, 0);
            var deliveryTag = Interlocked.Exchange(ref this.lastProcessed, 0);
            if (deliveryTag != 0)
            {
                await this.acknowledgement.AckAsync((ulong)deliveryTag, true);
            }
        }

    }
}