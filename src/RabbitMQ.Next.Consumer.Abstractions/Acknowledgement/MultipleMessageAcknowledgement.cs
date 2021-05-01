using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer.Abstractions.Acknowledgement
{
    internal class MultipleMessageAcknowledgement : IAcknowledgement
    {
        private readonly IAcknowledgement acknowledgement;
        private readonly CancellationTokenSource cts;
        private readonly int pendingLimit;
        private readonly int timeoutMs;
        private long lastProcessed;
        private int pendingCount;

        public MultipleMessageAcknowledgement(IAcknowledgement acknowledgement, TimeSpan timeout, int count)
        {
            this.acknowledgement = acknowledgement;
            this.cts = new CancellationTokenSource();
            this.pendingLimit = count;
            this.timeoutMs = (int)timeout.TotalMilliseconds;
        }

        public async ValueTask AckAsync(ulong deliveryTag, bool multiple = false)
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
#pragma warning disable 4014
                Task.Run(async () =>
                {
                    await Task.Delay(this.timeoutMs, this.cts.Token)
                        .ContinueWith(c => { }); // use empty continuation to suppress the TaskCancelledException
                    if (!this.cts.Token.IsCancellationRequested)
                    {
                        await this.SendPendingAck();
                    }
                });
#pragma warning restore 4014
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
                throw new ObjectDisposedException(nameof(MultipleMessageAcknowledgement));
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