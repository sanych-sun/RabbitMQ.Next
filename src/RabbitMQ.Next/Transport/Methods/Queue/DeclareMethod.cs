using System.Collections.Generic;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    public readonly struct DeclareMethod : IOutgoingMethod
    {
        public DeclareMethod(string queue, bool durable, bool exclusive, bool autoDelete, IReadOnlyDictionary<string, object> arguments)
            : this(queue, BitConverter.ComposeFlags(false, durable, exclusive, autoDelete), arguments)
        {}

        public DeclareMethod(string queue)
            : this(queue, BitConverter.ComposeFlags(true), null)
        {

        }

        public DeclareMethod(string queue, byte flags, IReadOnlyDictionary<string, object> arguments)
        {
            this.Queue = queue;
            this.Flags = flags;
            this.Arguments = arguments;
        }

        public MethodId MethodId => MethodId.QueueDeclare;

        public string Queue { get; }

        public byte Flags { get; }

        public IReadOnlyDictionary<string, object> Arguments { get; }
    }
}