using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.TopologyBuilder.Exceptions;
using RabbitMQ.Next.Transport.Methods.Exchange;

namespace RabbitMQ.Next.TopologyBuilder.Commands
{
    internal class ExchangeDeclareCommand : IExchangeBuilder, ICommand
    {
        private bool isDurable = true;
        private bool isInternal = false;
        private bool isAutoDelete = false;
        private Dictionary<string, object> arguments;

        public ExchangeDeclareCommand(string name, string type)
        {
            this.Name = name;
            this.Type = type;
        }

        public string Name { get; }

        public string Type { get; }

        public IExchangeBuilder Transient()
        {
            this.isDurable = false;
            return this;
        }

        public IExchangeBuilder Internal()
        {
            this.isInternal = true;
            return this;
        }

        public IExchangeBuilder AutoDelete()
        {
            this.isAutoDelete = true;
            return this;
        }


        public IExchangeBuilder Argument(string key, object value)
        {
            this.arguments ??= new Dictionary<string, object>();
            this.arguments[key] = value;

            return this;
        }

        public async Task ExecuteAsync(IChannel channel)
        {
            try
            {
                await channel.SendAsync<DeclareMethod, DeclareOkMethod>(
                    new (this.Name, this.Type, this.isDurable, this.isAutoDelete, this.isInternal, this.arguments));
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