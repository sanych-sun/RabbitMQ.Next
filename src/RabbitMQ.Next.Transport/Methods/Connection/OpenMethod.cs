using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    public readonly struct OpenMethod : IOutgoingMethod
    {
        public OpenMethod(string virtualHost)
        {
            this.VirtualHost = virtualHost;
        }

        public uint Method => (uint) MethodId.ConnectionOpen;

        public string VirtualHost { get; }

    }
}