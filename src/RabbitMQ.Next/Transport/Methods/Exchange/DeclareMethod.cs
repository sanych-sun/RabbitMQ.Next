using System.Collections.Generic;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    public readonly struct DeclareMethod : IOutgoingMethod
    {
        public DeclareMethod(string exchange, string type, bool durable, bool autoDelete, bool @internal, IReadOnlyDictionary<string, object> arguments)
        {
            this.Exchange = exchange;
            this.Type = type;
            this.Passive = false;
            this.Durable = durable;
            this.AutoDelete = autoDelete;
            this.Internal = @internal;
            this.Arguments = arguments;
        }

        public DeclareMethod(string exchange)
        {
            this.Exchange = exchange;
            this.Passive = true;
            this.Type = null;
            this.Durable = false;
            this.AutoDelete = false;
            this.Internal = false;
            this.Arguments = null;
        }

        public MethodId MethodId => MethodId.ExchangeDeclare;

        public string Exchange { get; }

        public string Type { get; }

        public bool Passive { get; }

        public bool Durable { get; }

        public bool AutoDelete { get; }

        public bool Internal { get; }

        public IReadOnlyDictionary<string, object> Arguments { get; }
    }
}