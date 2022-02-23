using System;

namespace RabbitMQ.Next.Publisher.Initializers
{
    public class ApplicationIdInitializer : IMessageInitializer
    {
        private readonly string applicationId;

        public ApplicationIdInitializer(string applicationId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
            {
                throw new ArgumentNullException(nameof(applicationId));
            }

            this.applicationId = applicationId;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
            => message.ApplicationId(this.applicationId);
    }
}