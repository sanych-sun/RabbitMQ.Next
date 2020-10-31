using System;

namespace RabbitMQ.Next.Abstractions
{
    public class ConnectionStateEventArgs : EventArgs
    {
        public ConnectionStateEventArgs(ConnectionState state)
        {
            this.State = state;
        }

        public ConnectionState State { get; }
    }
}