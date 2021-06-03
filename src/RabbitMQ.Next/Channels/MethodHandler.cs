using System;
using System.Buffers;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Channels
{
    public class MethodHandler<TMethod> : IMethodHandler
        where TMethod : IIncomingMethod
    {
        private readonly Func<TMethod, IMessageProperties, ReadOnlySequence<byte>, ValueTask<bool>> handler;

        public MethodHandler(Func<TMethod, IMessageProperties, ReadOnlySequence<byte>, ValueTask<bool>> handler)
        {
            this.handler = handler;
        }

        public ValueTask<bool> HandleAsync(IIncomingMethod method, IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
        {
            if (method is TMethod expectedMethod)
            {
                return this.handler(expectedMethod, properties, contentBytes);
            }

            return new ValueTask<bool>(false);
        }
    }
}