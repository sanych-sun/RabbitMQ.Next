using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer.Abstractions.Acknowledgement
{
    internal class EachMessageAcknowledgement : IAcknowledgement
    {
        private IAcknowledgement acknowledgement;

        public EachMessageAcknowledgement(IAcknowledgement acknowledgement)
        {
            if (acknowledgement == null)
            {
                throw new ArgumentNullException(nameof(acknowledgement));
            }

            this.acknowledgement = acknowledgement;
        }

        public ValueTask AckAsync(ulong deliveryTag, bool multiple = false)
        {
            this.CheckDisposed();
            return this.acknowledgement.AckAsync(deliveryTag, multiple);
        }

        public ValueTask NackAsync(ulong deliveryTag, bool requeue)
        {
            this.CheckDisposed();
            return this.acknowledgement.NackAsync(deliveryTag, requeue);
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
                throw new ObjectDisposedException(nameof(EachMessageAcknowledgement));
            }
        }
    }
}