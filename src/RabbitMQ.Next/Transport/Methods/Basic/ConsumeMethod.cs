using System.Collections.Generic;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic;

public readonly struct ConsumeMethod : IOutgoingMethod
{
    public ConsumeMethod(
        string queue, string consumerTag,
        bool noLocal, bool noAck, bool exclusive,
        IReadOnlyDictionary<string, object> arguments)
    {
        this.Queue = queue;
        this.ConsumerTag = consumerTag;
        this.NoLocal = noLocal;
        this.NoAck = noAck;
        this.Exclusive = exclusive;
        this.Arguments = arguments;
    }

    public MethodId MethodId => MethodId.BasicConsume;

    public string Queue { get; }

    public string ConsumerTag { get; }

    public bool NoLocal { get; }
    
    public bool NoAck { get; }
    
    public bool Exclusive { get; }

    public IReadOnlyDictionary<string, object> Arguments { get; }
}