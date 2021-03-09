using System;
using System.Collections.Generic;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization
{
    public class CombinedFormatterSource : IFormatterSource
    {
        private readonly IReadOnlyList<IFormatterSource> sources;

        public CombinedFormatterSource(IReadOnlyList<IFormatterSource> sources)
        {
            if (sources == null)
            {
                throw new ArgumentNullException(nameof(sources));
            }

            if (sources.Count == 0)
            {
                throw new ArgumentException(nameof(sources));
            }

            this.sources = sources;
        }

        public bool TryGetFormatter<TContent>(out ITypeFormatter typeFormatter)
        {
            for (var i = 0; i < this.sources.Count; i++)
            {
                if (this.sources[i].TryGetFormatter<TContent>(out var f1))
                {
                    typeFormatter = f1;
                    return true;
                }
            }

            typeFormatter = null;
            return false;
        }
    }
}