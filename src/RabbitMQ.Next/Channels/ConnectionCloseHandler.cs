using System.Buffers;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods.Connection;

namespace RabbitMQ.Next.Channels
{
    internal class ConnectionCloseHandler : IMethodHandler
    {
        private readonly IChannelInternal channel;

        public ConnectionCloseHandler(IChannelInternal channel)
        {
            this.channel = channel;
        }

        public ValueTask<bool> HandleAsync(IIncomingMethod method, IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
        {
            if (method is CloseMethod closeMethod)
            {
                this.channel.SetCompleted(new ConnectionException(closeMethod.StatusCode, closeMethod.Description));
                return new ValueTask<bool>(true);
            }

            return new ValueTask<bool>(false);
        }
    }
}