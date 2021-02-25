using RabbitMQ.Next.Publisher.Abstractions.Transformers;

namespace RabbitMQ.Next.Publisher.Transformers
{
    public class ApplicationIdTransformer : IMessageTransformer
    {
        private readonly string applicationId;

        public ApplicationIdTransformer(string applicationId)
        {
            this.applicationId = applicationId;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
        {
            if (string.IsNullOrEmpty(message.ApplicationId))
            {
                message.SetApplicationId(this.applicationId);
            }
        }

    }
}