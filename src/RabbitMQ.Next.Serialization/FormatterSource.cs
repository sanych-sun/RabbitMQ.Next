using System;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization
{
    public class FormatterSource : IFormatterSource
    {
        private readonly ITypeFormatter wrappedTypeFormatter;

        public FormatterSource(ITypeFormatter typeFormatter)
        {
            if (typeFormatter == null)
            {
                throw new ArgumentNullException(nameof(typeFormatter));
            }

            this.wrappedTypeFormatter = typeFormatter;
        }

        public bool TryGetFormatter<TContent>(out ITypeFormatter typeFormatter)
        {
            if (this.wrappedTypeFormatter.CanHandle(typeof(TContent)))
            {
                typeFormatter = this.wrappedTypeFormatter;
                return true;
            }

            typeFormatter = null;
            return false;
        }
    }
}