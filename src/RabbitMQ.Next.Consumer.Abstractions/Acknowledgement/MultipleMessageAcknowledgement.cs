using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer.Abstractions.Acknowledgement
{
    public class MultipleMessageAcknowledgement : IAcknowledgement
    {
        private readonly IAcknowledgement acknowledgement;
        private readonly CancellationTokenSource cts;
        private long lastProcessed;

        public MultipleMessageAcknowledgement(IAcknowledgement acknowledgement, TimeSpan timeout)
        {
            this.acknowledgement = acknowledgement;
            this.cts = new CancellationTokenSource();

            this.Worker(timeout, this.cts.Token);
        }

        public ValueTask AckAsync(ulong deliveryTag, bool multiple = false)
        {
            this.CheckDisposed();

            Interlocked.Exchange(ref this.lastProcessed, (long)deliveryTag);
            return default;
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

        private async Task Worker(TimeSpan timeout, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(timeout, cancellationToken);
                if (!cancellationToken.IsCancellationRequested)
                {
                    await this.SendPendingAck();
                }
            }
        }

        private async ValueTask SendPendingAck()
        {
            var deliveryTag = Interlocked.Exchange(ref this.lastProcessed, 0);
            if (deliveryTag != 0)
            {
                await this.acknowledgement.AckAsync((ulong)deliveryTag, true);
            }
        }


    }
}