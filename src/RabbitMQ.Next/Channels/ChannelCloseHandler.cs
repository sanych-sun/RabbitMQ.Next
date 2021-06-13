using System.Buffers;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods.Channel;

namespace RabbitMQ.Next.Channels
{
    internal class ChannelCloseHandler : IMethodHandler
    {
        private readonly IChannelInternal channel;

        public ChannelCloseHandler(IChannelInternal channel)
        {
            this.channel = channel;
        }

        public ValueTask<bool> HandleAsync(IIncomingMethod method, IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
        {
            if (method is CloseMethod closeMethod)
            {
                return this.HandleInternalAsync(closeMethod);
            }

            return new ValueTask<bool>(false);
        }

        private async ValueTask<bool> HandleInternalAsync(CloseMethod closeMethod)
        {
            await this.channel.SendAsync(new CloseOkMethod());

            this.channel.SetCompleted(new ChannelException(closeMethod.StatusCode, closeMethod.Description, closeMethod.MethodId));

            return true;
        }
    }
}