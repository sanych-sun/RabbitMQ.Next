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
        private QueueFlags flags;
        private Dictionary<string, object> arguments;

        public QueueDeclareCommand(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public IQueueBuilder Flags(QueueFlags flag)
        {
            this.flags |= flag;

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
                    new(this.Name, (byte)this.flags, this.arguments));
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