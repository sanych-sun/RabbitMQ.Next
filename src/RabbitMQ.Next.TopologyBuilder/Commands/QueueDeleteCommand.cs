using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Exceptions;
using RabbitMQ.Next.TopologyBuilder.Exceptions;
using RabbitMQ.Next.Transport.Methods.Queue;

namespace RabbitMQ.Next.TopologyBuilder.Commands
{
    internal class QueueDeleteCommand : IQueueDeleteBuilder, ICommand
    {
        private bool cancelConsumers;
        private bool discardMessages;

        public QueueDeleteCommand(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public IQueueDeleteBuilder CancelConsumers()
        {
            this.cancelConsumers = true;

            return this;
        }

        public IQueueDeleteBuilder DiscardMessages()
        {
            this.discardMessages = true;

            return this;
        }

        public async Task ExecuteAsync(IChannel channel)
        {
            try
            {
                await channel.SendAsync<DeleteMethod, DeleteOkMethod>(
                    new (this.Name, !this.cancelConsumers, !this.discardMessages));
            }
            catch (ChannelException ex)
            {
                switch (ex.ErrorCode)
                {
                    case (ushort)ReplyCode.NotFound:
                        throw new ArgumentOutOfRangeException("Specified queue does not exist.", ex);
                    case (ushort)ReplyCode.PreconditionFailed:
                        throw new ConflictException("Specified queue is in use.", ex);
                }
                throw;
            }
        }
    }
}