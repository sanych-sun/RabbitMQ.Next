using System.Collections.Generic;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    public readonly struct StartOkMethod : IOutgoingMethod
    {
        public StartOkMethod(string mechanism, string response, string locale, IReadOnlyDictionary<string, object> clientProperties)
        {
            this.Mechanism = mechanism;
            this.Response = response;
            this.Locale = locale;
            this.ClientProperties = clientProperties;
        }

        public MethodId MethodId => MethodId.ConnectionStartOk;

        public IReadOnlyDictionary<string, object> ClientProperties { get; }

        public string Mechanism { get; }

        public string Response { get; }

        public string Locale { get; }
    }
}