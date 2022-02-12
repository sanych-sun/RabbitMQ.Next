using System.Collections.Generic;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    public readonly struct DeclareMethod : IOutgoingMethod
    {
        public DeclareMethod(string queue, bool durable, bool exclusive, bool autoDelete, IReadOnlyDictionary<string, object> arguments)
        {
            this.Queue = queue;
            this.Arguments = arguments;
            this.Passive = false;
            this.Durable = durable;
            this.Exclusive = exclusive;
            this.AutoDelete = autoDelete;
        }

        public DeclareMethod(string queue)
        {
            this.Queue = queue;
            this.Arguments = null;
            this.Passive = true;
            this.Durable = false;
            this.Exclusive = false;
            this.AutoDelete = false;
        }


        public MethodId MethodId => MethodId.QueueDeclare;

        public string Queue { get; }

        public bool Passive { get; }

        public bool Durable { get; }

        public bool AutoDelete { get; }

        public bool Exclusive { get; }

        public IReadOnlyDictionary<string, object> Arguments { get; }
    }
}