using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic;

public readonly struct ReturnMethod : IIncomingMethod
{
    public ReturnMethod(string exchange, string routingKey, ushort replyCode, string replyText)
    {
        this.Exchange = exchange;
        this.RoutingKey = routingKey;
        this.ReplyCode = replyCode;
        this.ReplyText = replyText;
    }

    public MethodId MethodId => MethodId.BasicReturn;

    public string Exchange { get; }

    public string RoutingKey { get; }

    public ushort ReplyCode { get; }

    public string ReplyText { get; }
}