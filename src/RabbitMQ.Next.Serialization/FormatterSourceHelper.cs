using System;
using System.Collections.Generic;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization
{
    public static class FormatterSourceHelper
    {
        public static IFormatterSource CombineFormatters(IReadOnlyList<IFormatter> formatters, IReadOnlyList<IFormatterSource> sources)
        {
            IFormatterSource result = null;
            if (formatters != null && formatters.Count > 0)
            {
                if (formatters.Count == 1)
                {
                    result = new SingleFormatterSource(formatters[0]);
                }
                else
                {
                    result = new FormatterSource(formatters);
                }
            }

            if (sources == null || sources.Count == 0)
            {
                return result;
            }

            if (result == null && sources.Count == 1)
            {
                return sources[0];
            }

            if (result != null)
            {
                var newsources = new List<IFormatterSource>(sources);
                newsources.Insert(0, result);

                sources = newsources;
            }

            return new CombinedFormatterSource(sources);
        }
    }
}