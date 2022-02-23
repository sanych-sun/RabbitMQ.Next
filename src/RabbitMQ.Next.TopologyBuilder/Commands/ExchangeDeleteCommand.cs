using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Exceptions;
using RabbitMQ.Next.TopologyBuilder.Exceptions;
using RabbitMQ.Next.Transport.Methods.Exchange;

namespace RabbitMQ.Next.TopologyBuilder.Commands
{
    internal class ExchangeDeleteCommand : IExchangeDeleteBuilder, ICommand
    {
        private bool cancelBindings;

        public ExchangeDeleteCommand(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public IExchangeDeleteBuilder CancelBindings()
        {
            this.cancelBindings = true;

            return this;
        }

        public async Task ExecuteAsync(IChannel channel)
        {
            try
            {
                await channel.SendAsync<DeleteMethod, DeleteOkMethod>(
                    new DeleteMethod(this.Name, !this.cancelBindings));
            }
            catch (ChannelException ex)
            {
                switch (ex.ErrorCode)
                {
                    case (ushort)ReplyCode.NotFound:
                        throw new ArgumentOutOfRangeException("Specified exchange does not exist.", ex);
                    case (ushort)ReplyCode.PreconditionFailed:
                        throw new ConflictException("Specified exchange is in use. Try to remove bindings or call CancelBindings.", ex);
                }
                throw;
            }
        }
    }
}