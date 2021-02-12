using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization
{
    public class SingleFormatterSource : IFormatterSource
    {
        private readonly IFormatter wrappedFormatter;

        public SingleFormatterSource(IFormatter formatter)
        {
            this.wrappedFormatter = formatter;
        }

        public IFormatter GetFormatter<TContent>()
        {
            if (this.wrappedFormatter.CanHandle(typeof(TContent)))
            {
                return this.wrappedFormatter;
            }

            return null;
        }
    }
}