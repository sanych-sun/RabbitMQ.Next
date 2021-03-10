using System.Collections.Generic;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    public readonly struct BindMethod : IOutgoingMethod
    {
        public BindMethod(string queue, string exchange, string routingKey, IReadOnlyDictionary<string, object> arguments)
        {
            this.Queue = queue;
            this.Exchange = exchange;
            this.RoutingKey = routingKey;
            this.Arguments = arguments;
        }

        public uint Method => (uint) MethodId.QueueBind;

        public string Queue { get; }

        public string Exchange { get; }

        public string RoutingKey { get; }

        public IReadOnlyDictionary<string, object> Arguments { get; }

    }
}