using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Transport.Methods.Queue;

namespace RabbitMQ.Next.TopologyBuilder.Commands
{
    internal class QueuePurgeCommand : ICommand
    {
        private readonly string queueName;

        public QueuePurgeCommand(string queueName)
        {
            this.queueName = queueName;
        }

        public async Task ExecuteAsync(IChannel channel)
        {
            try
            {
                await channel.SendAsync<PurgeMethod, PurgeOkMethod>(
                    new(this.queueName));
            }
            catch (ChannelException ex)
            {
                switch (ex.ErrorCode)
                {
                    case (ushort)ReplyCode.NotFound:
                        throw new ArgumentOutOfRangeException("Specified queue does not exist.", ex);
                }
                throw;
            }
        }
    }
}