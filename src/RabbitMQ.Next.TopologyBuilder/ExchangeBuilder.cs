using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.TopologyBuilder.Abstractions;
using RabbitMQ.Next.TopologyBuilder.Abstractions.Exceptions;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Exchange;

namespace RabbitMQ.Next.TopologyBuilder
{
    internal class ExchangeBuilder : IExchangeBuilder
    {
        private Dictionary<string, object> arguments;

        public ExchangeBuilder(string name, string type)
        {
            this.Name = name;
            this.Type = type;
        }

        public string Name { get; }

        public string Type { get; }

        public ExchangeFlags Flags { get; set; }

        public void SetArgument(string key, object value)
        {
            this.arguments ??= new Dictionary<string, object>();
            this.arguments[key] = value;
        }

        public DeclareMethod ToMethod()
            => new DeclareMethod(this.Name, this.Type, (byte)this.Flags, this.arguments);

        public async Task ApplyAsync(IChannel channel)
        {
            try
            {
                await channel.SendAsync<DeclareMethod, DeclareOkMethod>(this.ToMethod());
            }
            catch (ChannelException ex)
            {
                switch (ex.ErrorCode)
                {
                    case (ushort)ReplyCode.AccessRefused:
                    case (ushort)ReplyCode.PreconditionFailed:
                        throw new ArgumentOutOfRangeException("Illegal exchange name", ex);
                    case (ushort)ReplyCode.NotAllowed:
                        throw new ConflictException("Exchange cannot be redeclared with different type", ex);
                    case (ushort)ReplyCode.CommandInvalid:
                        throw new NotSupportedException("Specified exchange type does not supported", ex);
                }
                throw;
            }
        }
    }
}