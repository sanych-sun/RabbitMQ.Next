using System;

namespace RabbitMQ.Next.Publisher.Initializers
{
    public class HeaderInitializer : IMessageInitializer
    {
        private readonly string key;
        private readonly string value;

        public HeaderInitializer(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            this.key = key;
            this.value = value;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
            => message.SetHeader(this.key, this.value);
    }
}