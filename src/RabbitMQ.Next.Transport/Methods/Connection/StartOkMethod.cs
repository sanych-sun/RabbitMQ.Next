using System.Collections.Generic;

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

        public uint Method => (uint) MethodId.ConnectionStartOk;

        public IReadOnlyDictionary<string, object> ClientProperties { get; }

        public string Mechanism { get; }

        public string Response { get; }

        public string Locale { get; }
    }
}