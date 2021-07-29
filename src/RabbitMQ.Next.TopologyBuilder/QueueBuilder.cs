using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.TopologyBuilder.Abstractions;
using RabbitMQ.Next.TopologyBuilder.Abstractions.Exceptions;
using RabbitMQ.Next.Transport.Methods.Queue;

namespace RabbitMQ.Next.TopologyBuilder
{
    internal class QueueBuilder : IQueueBuilder
    {
        private Dictionary<string, object> arguments;

        public QueueBuilder(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public QueueFlags Flags { get; set; }

        public void SetArgument(string key, object value)
        {
            this.arguments ??= new Dictionary<string, object>();
            this.arguments[key] = value;
        }

        public DeclareMethod ToMethod() => new(this.Name, (byte)this.Flags, this.arguments);

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
                        throw new ArgumentOutOfRangeException("Illegal queue name.", ex);
                    case (ushort)ReplyCode.ResourceLocked:
                        throw new ConflictException("Cannot redeclare existing queue as exclusive.", ex);
                }
                throw;
            }
        }
    }
}