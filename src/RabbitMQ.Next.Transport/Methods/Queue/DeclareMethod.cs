using System.Collections.Generic;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    public readonly struct DeclareMethod : IOutgoingMethod
    {

        // Durable = (1 << 1),
        // Exclusive = (1 << 2),
        // AutoDelete = (1 << 3),

        public DeclareMethod(string queue, bool passive, bool durable, bool exclusive, bool autoDelete, bool nowait,IReadOnlyDictionary<string, object> arguments)
            : this(queue, BitConverter.ComposeFlags(passive, durable, exclusive, autoDelete, nowait), arguments)
        {}

        public DeclareMethod(string queue, byte flags, IReadOnlyDictionary<string, object> arguments)
        {
            this.Queue = queue;
            this.Flags = flags;
            this.Arguments = arguments;
        }

        public uint Method => (uint) MethodId.QueueDeclare;

        public string Queue { get; }

        public byte Flags { get; }

        public IReadOnlyDictionary<string, object> Arguments { get; }
    }
}