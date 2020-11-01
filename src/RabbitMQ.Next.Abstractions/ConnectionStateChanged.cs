namespace RabbitMQ.Next.Abstractions
{
    public readonly struct ConnectionStateChanged
    {
        public ConnectionStateChanged(ConnectionState state)
        {
            this.State = state;
        }

        public ConnectionState State { get; }
    }
}