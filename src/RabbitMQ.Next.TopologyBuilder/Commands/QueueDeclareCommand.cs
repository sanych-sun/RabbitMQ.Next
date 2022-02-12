using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.TopologyBuilder.Exceptions;
using RabbitMQ.Next.Transport.Methods.Queue;

namespace RabbitMQ.Next.TopologyBuilder.Commands
{
    internal class QueueDeclareCommand : IQueueBuilder, ICommand
    {
        private bool isDurable = true;
        private bool isExclusive = false;
        private bool isAutoDelete = false;
        private Dictionary<string, object> arguments;

        public QueueDeclareCommand(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public IQueueBuilder Transient()
        {
            this.isDurable = false;
            return this;
        }

        public IQueueBuilder Exclusive()
        {
            this.isExclusive = true;
            return this;
        }

        public IQueueBuilder AutoDelete()
        {
            this.isAutoDelete = true;
            return this;
        }

        public IQueueBuilder Argument(string key, object value)
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
                    new(this.Name, this.isDurable, this.isExclusive, this.isAutoDelete, this.arguments));
            }
            catch (ChannelException ex)
            {
                switch (ex.ErrorCode)
                {
                    case (ushort)ReplyCode.AccessRefused:
                    case (ushort)ReplyCode.PreconditionFailed:
                        throw new ArgumentOutOfRangeException("Illegal queue name.", ex);
                    case (ushort)ReplyCode.ResourceLocked:
                        throw new ConflictException("Cannot redeclare existing queue as exclusive.", ex);
                }
                throw;
            }
        }
    }
}