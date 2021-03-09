using System;
using System.Collections.Generic;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization
{
    public class MultipleFormatterSource : IFormatterSource
    {
        private readonly IReadOnlyList<ITypeFormatter> formatters;

        public MultipleFormatterSource(IReadOnlyList<ITypeFormatter> formatters)
        {
            if (formatters == null)
            {
                throw new ArgumentNullException(nameof(formatters));
            }

            if (formatters.Count == 0)
            {
                throw new ArgumentException(nameof(formatters));
            }

            this.formatters = formatters;
        }

        public bool TryGetFormatter<TContent>(out ITypeFormatter typeFormatter)
        {
            foreach (var item in this.formatters)
            {
                if (item.CanHandle(typeof(TContent)))
                {
                    typeFormatter = item;
                    return true;
                }
            }

            typeFormatter = null;
            return false;
        }
    }
}