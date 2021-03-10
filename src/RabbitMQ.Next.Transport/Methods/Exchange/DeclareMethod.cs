using System.Collections.Generic;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    public readonly struct DeclareMethod : IOutgoingMethod
    {
        public DeclareMethod(string exchange, string type, bool passive, bool durable, bool autoDelete, bool @internal, bool nowait, IReadOnlyDictionary<string, object> arguments)
            : this(exchange, type, BitConverter.ComposeFlags(passive, durable, autoDelete, @internal, nowait), arguments)
        {
        }

        public DeclareMethod(string exchange, string type, byte flags, IReadOnlyDictionary<string, object> arguments)
        {
            this.Exchange = exchange;
            this.Type = type;
            this.Flags = flags;
            this.Arguments = arguments;
        }

        public uint Method => (uint) MethodId.ExchangeDeclare;

        public string Exchange { get; }

        public string Type { get; }

        public byte Flags { get; }

        public IReadOnlyDictionary<string, object> Arguments { get; }
    }
}