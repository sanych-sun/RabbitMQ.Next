using System.Collections.Generic;
using RabbitMQ.Next.Abstractions;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    public readonly struct DeclareMethod : IOutgoingMethod
    {
        public DeclareMethod(string exchange, string type, ExchangeFlags flags, IReadOnlyDictionary<string, object> arguments)
        {
            this.Exchange = exchange;
            this.Type = type;
            this.Flags = flags;
            this.Arguments = arguments;
        }

        public uint Method => (uint) MethodId.ExchangeDeclare;

        public string Exchange { get; }

        public string Type { get; }

        public ExchangeFlags Flags { get; }

        public IReadOnlyDictionary<string, object> Arguments { get; }
    }
}