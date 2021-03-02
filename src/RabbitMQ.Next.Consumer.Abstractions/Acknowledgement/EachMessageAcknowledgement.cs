using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer.Abstractions.Acknowledgement
{
    public class EachMessageAcknowledgement : IAcknowledgement
    {
        private readonly IAcknowledgement acknowledgement;
        
        public EachMessageAcknowledgement(IAcknowledgement acknowledgement)
        {
            if (acknowledgement == null)
            {
                throw new ArgumentNullException(nameof(acknowledgement));
            }

            this.acknowledgement = acknowledgement;
        }

        public ValueTask AckAsync(ulong deliveryTag, bool multiple = false)
            => this.acknowledgement.AckAsync(deliveryTag, multiple);

        public ValueTask NackAsync(ulong deliveryTag, bool requeue)
            => this.acknowledgement.NackAsync(deliveryTag, requeue);


        public ValueTask DisposeAsync()
            => this.acknowledgement.DisposeAsync();
    }
}