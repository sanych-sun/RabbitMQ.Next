using System.Collections.Generic;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next
{
    internal class ConnectionSettings
    {
        public IReadOnlyList<Endpoint> Endpoints { get; set; }

        public string Vhost { get; set; }

        public string Locale { get; set; }

        public IAuthMechanism Auth { get; set; }

        public int MaxFrameSize { get; set; }

        public IReadOnlyDictionary<string, object> ClientProperties { get; set; }
    }
}