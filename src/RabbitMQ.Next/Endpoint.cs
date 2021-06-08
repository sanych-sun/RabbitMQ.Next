namespace RabbitMQ.Next
{
    internal readonly struct Endpoint
    {
        public Endpoint(string host, int port, bool useSsl)
        {
            this.Host = host;
            this.Port = port;
            this.UseSsl = useSsl;
        }

        public string Host { get; }

        public int Port { get; }

        public bool UseSsl { get; }
    }
}