using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class ApplicationIdTransformer : IMessageTransformer
    {
        private readonly string applicationId;

        public ApplicationIdTransformer(string applicationId)
        {
            this.applicationId = applicationId;
        }

        public void Apply<TPayload>(TPayload payload, MessageHeader header)
        {
            if (string.IsNullOrEmpty(header.Properties.ApplicationId))
            {
                header.Properties.ApplicationId = this.applicationId;
            }
        }
    }
}