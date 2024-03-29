using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection;

public readonly struct OpenMethod : IOutgoingMethod
{
    public OpenMethod(string virtualHost)
    {
        this.VirtualHost = virtualHost;
    }

    public MethodId MethodId => MethodId.ConnectionOpen;

    public string VirtualHost { get; }

}