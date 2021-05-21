using System.Buffers;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Channel;

namespace RabbitMQ.Next.Transport.Channels
{
    internal class ChannelCloseHandler : IFrameHandler
    {
        private readonly IMethodParser<CloseMethod> parser;

        public ChannelCloseHandler(IMethodRegistry registry, IChannelInternal channel)
        {
            this.parser = registry.GetParser<CloseMethod>();
        }

        public bool Handle(ChannelFrameType type, ReadOnlySequence<byte> payload)
        {
            if (type != ChannelFrameType.Method)
            {
                return false;
            }

            payload = payload.Read(out uint methodId);

            if (methodId != (uint) MethodId.ChannelClose)
            {
                return false;
            }

            var closeMethod = this.parser.Parse(payload);


            return true;
        }
    }
}