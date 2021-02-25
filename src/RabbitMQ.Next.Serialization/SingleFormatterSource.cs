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

        public bool TryGetFormatter<TContent>(out IFormatter formatter)
        {
            if (this.wrappedFormatter.CanHandle(typeof(TContent)))
            {
                formatter = this.wrappedFormatter;
                return true;
            }

            formatter = null;
            return false;
        }
    }
}