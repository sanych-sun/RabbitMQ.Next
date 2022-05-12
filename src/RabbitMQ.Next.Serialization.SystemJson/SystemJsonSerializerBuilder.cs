using System;
using System.Collections.Generic;
using System.Text.Json;

namespace RabbitMQ.Next.Serialization.SystemJson
{
    internal class SystemJsonSerializerBuilder: ISystemJsonSerializerBuilder
    {
        private static readonly string[] DefaultContentTypes = { "application/json" };
        private List<string> contentTypes;

        public bool IsDefault { get; private set; } = true;

        public IReadOnlyList<string> ContentTypes
            => this.contentTypes == null ? DefaultContentTypes : this.contentTypes;

        public JsonSerializerOptions Options { get; } = new();

        public ISystemJsonSerializerBuilder AsDefault()
        {
            this.IsDefault = true;
            return this;
        }

        public ISystemJsonSerializerBuilder ContentType(string contentType)
        {
            this.contentTypes ??= new List<string>(DefaultContentTypes);
            this.contentTypes.Add(contentType);

            return this;
        }

        public ISystemJsonSerializerBuilder Configure(Action<JsonSerializerOptions> func)
        {
            func?.Invoke(this.Options);
            return this;
        }
    }
}