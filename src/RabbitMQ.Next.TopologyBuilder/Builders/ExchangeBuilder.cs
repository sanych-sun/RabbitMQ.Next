using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.TopologyBuilder.Exceptions;
using RabbitMQ.Next.Transport.Methods.Exchange;

namespace RabbitMQ.Next.TopologyBuilder.Builders
{
    internal class ExchangeBuilder : IExchangeBuilder
    {
        private ExchangeFlags flags;
        private Dictionary<string, object> arguments;

        public ExchangeBuilder(string name, string type)
        {
            this.Name = name;
            this.Type = type;
        }

        public string Name { get; }

        public string Type { get; }

        public IExchangeBuilder Flags(ExchangeFlags flag)
        {
            this.flags |= flag;

            return this;
        }

        public IExchangeBuilder Argument(string key, object value)
        {
            this.arguments ??= new Dictionary<string, object>();
            this.arguments[key] = value;

            return this;
        }

        public async Task ApplyAsync(IChannel channel)
        {
            try
            {
                await channel.SendAsync<DeclareMethod, DeclareOkMethod>(
                    new (this.Name, this.Type, (byte)this.flags, this.arguments));
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