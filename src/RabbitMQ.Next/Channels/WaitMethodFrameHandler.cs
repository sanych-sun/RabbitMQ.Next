using System;
using System.Buffers;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Channels
{
    internal class WaitMethodFrameHandler<TMethod> : IFrameHandler
        where TMethod: struct, IIncomingMethod
    {
        private readonly MethodId expectedMethodId;
        private readonly IMethodParser<TMethod> parser;
        private readonly TaskCompletionSource<TMethod> tcs;

        public WaitMethodFrameHandler(IMethodRegistry registry)
        {
            this.expectedMethodId = registry.GetMethodId<TMethod>();
            this.parser = registry.GetParser<TMethod>();
            this.tcs = new TaskCompletionSource<TMethod>();
        }

        public Task<TMethod> WaitTask => this.tcs.Task;

        public bool HandleMethodFrame(MethodId methodId, ReadOnlyMemory<byte> payload)
        {
            if (methodId != this.expectedMethodId)
            {
                return false;
            }

            var method = this.parser.Parse(payload);
            this.tcs.TrySetResult(method);

            return true;
        }

        public ValueTask<bool> HandleContentAsync(IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
            => new ValueTask<bool>(false);

        public void Reset()
        {
            this.tcs.TrySetCanceled();
        }
    }
}