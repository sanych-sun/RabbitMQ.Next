using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Serialization.PlainText.Formatters;

namespace RabbitMQ.Next.Serialization.PlainText
{
    public static class SerializationBuilderExtensions
    {
        public static TBuilder UsePlainTextSerializer<TBuilder>(this TBuilder builder, bool asDefaultSerializer = true)
            where TBuilder : ISerializationBuilder
        {
            var contentTypes = new List<string>{"text/plain"};
            if (asDefaultSerializer)
            {
                contentTypes.Add(string.Empty);
            }

            return builder.UsePlainTextSerializer(contentTypes);
        }

        public static TBuilder UsePlainTextSerializer<TBuilder>(this TBuilder builder, IEnumerable<string> contentTypes, IEnumerable<IFormatter> customFormatters = null)
            where TBuilder : ISerializationBuilder
        {
            var formatters = new List<IFormatter>();
            if (customFormatters != null)
            {
                formatters.AddRange(customFormatters);
            }

            formatters.Add(new StringFormatter());
            formatters.Add(new DateTimeFormatter());
            formatters.Add(new DateTimeOffsetFormatter());
            formatters.Add(new Int32Formatter());
            formatters.Add(new Int64Formatter());

            builder.AddSerializer(new PlainTextSerializer(formatters), contentTypes.ToArray());

            return builder;
        }
    }
}