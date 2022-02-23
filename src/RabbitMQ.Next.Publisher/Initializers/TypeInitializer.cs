using System;

namespace RabbitMQ.Next.Publisher.Initializers
{
    public class TypeInitializer : IMessageInitializer
    {
        private readonly string type;

        public TypeInitializer(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentNullException(nameof(type));
            }

            this.type = type;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
            => message.Type(this.type);
    }
}