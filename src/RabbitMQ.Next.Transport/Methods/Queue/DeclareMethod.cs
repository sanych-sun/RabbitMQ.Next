using System.Collections.Generic;
using RabbitMQ.Next.Abstractions;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    public readonly struct DeclareMethod : IOutgoingMethod
    {
        public DeclareMethod(string queue, QueueFlags flags, IReadOnlyDictionary<string, object> arguments)
        {
            this.Queue = queue;
            this.Flags = flags;
            this.Arguments = arguments;
        }

        public uint Method => (uint) MethodId.QueueDeclare;

        public string Queue { get; }

        public QueueFlags Flags { get; }

        public IReadOnlyDictionary<string, object> Arguments { get; }
    }
}