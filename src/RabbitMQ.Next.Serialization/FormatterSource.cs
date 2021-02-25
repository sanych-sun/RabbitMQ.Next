using System.Collections.Generic;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization
{
    public class FormatterSource : IFormatterSource
    {
        private readonly IReadOnlyList<IFormatter> formatters;

        public FormatterSource(IReadOnlyList<IFormatter> formatters)
        {
            this.formatters = formatters;
        }

        public bool TryGetFormatter<TContent>(out IFormatter formatter)
        {
            foreach (var item in this.formatters)
            {
                if (item.CanHandle(typeof(TContent)))
                {
                    formatter = item;
                    return true;
                }
            }

            formatter = null;
            return false;
        }
    }
}