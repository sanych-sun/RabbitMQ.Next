namespace RabbitMQ.Next.Publisher;

public readonly struct ReturnedMessage
{
    public ReturnedMessage(string exchange, string routingKey, ushort replyCode, string replyText)
    {
        this.Exchange = exchange;
        this.RoutingKey = routingKey;
        this.ReplyCode = replyCode;
        this.ReplyText = replyText;
    }

    public string Exchange { get; }

    public string RoutingKey { get; }

    public ushort ReplyCode { get; }

    public string ReplyText { get; }
}