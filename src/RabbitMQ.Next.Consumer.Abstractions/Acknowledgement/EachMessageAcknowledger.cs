using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer.Abstractions.Acknowledgement
{
    internal class EachMessageAcknowledger : IAcknowledger
    {
        private IAcknowledgement acknowledgement;

        public EachMessageAcknowledger(IAcknowledgement acknowledgement)
        {
            if (acknowledgement == null)
            {
                throw new ArgumentNullException(nameof(acknowledgement));
            }

            this.acknowledgement = acknowledgement;
        }

        public async ValueTask AckAsync(ulong deliveryTag)
        {
            this.CheckDisposed();
            await this.acknowledgement.AckAsync(deliveryTag);
        }

        public async ValueTask NackAsync(ulong deliveryTag, bool requeue)
        {
            this.CheckDisposed();
            await this.acknowledgement.NackAsync(deliveryTag, requeue);
        }


        public async ValueTask DisposeAsync()
        {
            if (this.acknowledgement == null)
            {
                return;
            }

            await this.acknowledgement.DisposeAsync();
            this.acknowledgement = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed()
        {
            if (this.acknowledgement == null)
            {
                throw new ObjectDisposedException(nameof(EachMessageAcknowledger));
            }
        }
    }
}