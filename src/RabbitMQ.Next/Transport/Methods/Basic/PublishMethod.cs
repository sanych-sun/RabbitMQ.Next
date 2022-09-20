using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic;

public readonly struct PublishMethod : IOutgoingMethod
{
    public PublishMethod(string exchange, string routingKey, bool mandatory, bool immediate)
    {
        this.Exchange = exchange;
        this.RoutingKey = routingKey;
        this.Mandatory = mandatory;
        this.Immediate = immediate;
    }

    public MethodId MethodId => MethodId.BasicPublish;

    public string Exchange { get; }

    public string RoutingKey { get; }

    public bool Mandatory { get; }
    
    public bool Immediate { get; }
}