using System;
using System.Buffers;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer
{
    internal sealed class DeliverFrameHandler : IFrameHandler
    {
        private readonly IMethodParser<DeliverMethod> deliverMethodParser;
        private readonly Func<DeliverMethod, IMessageProperties, ReadOnlySequence<byte>, ValueTask<bool>> methodHandler;

        private bool expectContent;
        private DeliverMethod currentMethod;

        public DeliverFrameHandler(IMethodRegistry registry, Func<DeliverMethod, IMessageProperties, ReadOnlySequence<byte>, ValueTask<bool>> methodHandler)
        {
            this.deliverMethodParser = registry.GetParser<DeliverMethod>();
            this.methodHandler = methodHandler;
        }

        public ValueTask<bool> HandleMethodFrameAsync(MethodId methodId, ReadOnlyMemory<byte> payload)
        {
            if (methodId != MethodId.BasicDeliver)
            {
                return new(false);
            }

            this.currentMethod = this.deliverMethodParser.Parse(payload);
            this.expectContent = true;
            return new(true);
        }

        public ValueTask<bool> HandleContentAsync(IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
        {
            if (!this.expectContent)
            {
                return new(false);
            }

            var method = this.currentMethod;
            this.expectContent = false;
            this.currentMethod = default;
            return this.methodHandler(method, properties, contentBytes);
        }
    }
}