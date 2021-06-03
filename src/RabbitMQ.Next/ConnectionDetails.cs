using RabbitMQ.Next.Abstractions;

namespace RabbitMQ.Next
{
    internal class ConnectionDetails : IConnectionDetails
    {
        public int FrameMaxSize { get; set; }

        public int HeartbeatInterval { get; set; }

        public string AuthMechanism { get; set; }

        public string RemoteHost { get; set; }

        public string RemotePort { get; set; }

        public bool IsSsl { get; set; }

        public string VirtualHost { get; set; }
    }
}