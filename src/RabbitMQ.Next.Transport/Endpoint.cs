namespace RabbitMQ.Next.Transport
{
    public readonly struct Endpoint
    {
        public Endpoint(string host, int port)
        {
            this.Host = host;
            this.Port = port;
        }

        public string Host { get; }

        public int Port { get; }
    }
}