namespace RabbitMQ.Next.Abstractions
{
    public interface IConnectionDetails
    {
        public int FrameMaxSize { get; }

        public int HeartbeatInterval { get; }

        public string AuthMechanism { get; }

        public string RemoteHost { get; }

        public string RemotePort { get; }

        public bool IsSsl { get; }

        public string VirtualHost { get; }
    }
}