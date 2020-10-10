using System.Collections.Generic;
using RabbitMQ.Next.Abstractions;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    public readonly struct DeclareMethod : IOutgoingMethod
    {
        public DeclareMethod(string queue, bool passive, QueueFlags flags, IReadOnlyDictionary<string, object> arguments)
        {
            this.Queue = queue;
            this.Passive = passive;
            this.Flags = flags;
            this.Arguments = arguments;
        }

        public uint Method => (uint) MethodId.QueueDeclare;

        public string Queue { get; }

        public bool Passive { get; }

        public QueueFlags Flags { get; }

        public IReadOnlyDictionary<string, object> Arguments { get; }
    }
}