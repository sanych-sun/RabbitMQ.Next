using System.Collections.Generic;
using RabbitMQ.Next.Serialization.PlainText.Formatters;

namespace RabbitMQ.Next.Serialization.PlainText
{
    internal class PlainTextSerializerBuilder : IPlainTextSerializerBuilder
    {
        private static readonly string[] DefaultContentTypes = { "text/plain" };
        private static readonly IFormatter[] DefaultFormatters = {
            new StringFormatter(),
            new DateTimeOffsetFormatter(),
            new Int32Formatter(),
            new Int64Formatter(),
        };

        private List<string> contentTypes;
        private List<IFormatter> formatters;
        public bool IsDefault { get; private set; } = true;

        public IReadOnlyList<string> ContentTypes
            => this.contentTypes == null ? DefaultContentTypes : this.contentTypes;

        public IReadOnlyList<IFormatter> Formatters
            => this.formatters == null ? DefaultFormatters : this.formatters;

        IPlainTextSerializerBuilder IPlainTextSerializerBuilder.AsDefault()
        {
            this.IsDefault = true;
            return this;
        }

        IPlainTextSerializerBuilder IPlainTextSerializerBuilder.ContentType(string contentType)
        {
            this.contentTypes ??= new List<string>(DefaultContentTypes);
            this.contentTypes.Add(contentType);

            return this;
        }

        IPlainTextSerializerBuilder IPlainTextSerializerBuilder.UseFormatter(IFormatter formatter)
        {
            this.formatters ??= new List<IFormatter>(DefaultFormatters);
            this.formatters.Add(formatter);

            return this;
        }
    }
}