using System.Collections.Generic;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic;

public readonly struct ConsumeMethod : IOutgoingMethod
{
    public ConsumeMethod(string queue, string consumerTag,
        bool noLocal, bool noAck, bool exclusive,
        IReadOnlyDictionary<string, object> arguments)
        : this(queue, consumerTag, BitConverter.ComposeFlags(noLocal, noAck, exclusive), arguments)
    {
    }

    public ConsumeMethod(string queue, string consumerTag, byte flags, IReadOnlyDictionary<string, object> arguments)
    {
        this.Queue = queue;
        this.ConsumerTag = consumerTag;
        this.Flags = flags;
        this.Arguments = arguments;
    }

    public MethodId MethodId => MethodId.BasicConsume;

    public string Queue { get; }

    public string ConsumerTag { get; }

    public byte Flags { get; }

    public IReadOnlyDictionary<string, object> Arguments { get; }
}