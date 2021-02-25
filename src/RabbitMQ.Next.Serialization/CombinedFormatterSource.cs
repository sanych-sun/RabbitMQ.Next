using System.Collections.Generic;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization
{
    public class CombinedFormatterSource : IFormatterSource
    {
        private readonly IReadOnlyList<IFormatterSource> sources;

        public CombinedFormatterSource(IReadOnlyList<IFormatterSource> sources)
        {
            this.sources = sources;
        }

        public bool TryGetFormatter<TContent>(out IFormatter formatter)
        {
            for (var i = 0; i < this.sources.Count; i++)
            {
                if (this.sources[i].TryGetFormatter<TContent>(out var f1))
                {
                    formatter = f1;
                    return true;
                }
            }

            formatter = null;
            return false;
        }
    }
}