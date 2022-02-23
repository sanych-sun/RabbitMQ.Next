namespace RabbitMQ.Next.Publisher.Initializers
{
    public class PriorityInitializer : IMessageInitializer
    {
        private readonly byte priority;

        public PriorityInitializer(byte priority)
        {
            this.priority = priority;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
            => message.Priority(this.priority);
    }
}