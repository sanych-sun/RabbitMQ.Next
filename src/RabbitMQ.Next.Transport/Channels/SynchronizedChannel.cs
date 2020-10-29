using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Channels
{
    internal class SynchronizedChannel : ISynchronizedChannel
    {
        private readonly IFrameSender frameSender;
        private readonly WaitMethodFrameHandler waitFrameHandler;

        public SynchronizedChannel(IFrameSender frameSender, WaitMethodFrameHandler waitFrameHandler)
        {
            this.frameSender = frameSender;
            this.waitFrameHandler = waitFrameHandler;
        }

        public Task SendAsync<TMethod>(TMethod request)
            where TMethod : struct, IOutgoingMethod
            => this.frameSender.SendMethodAsync(request);

        public async Task SendAsync<TMethod>(TMethod request, MessageProperties properties, ReadOnlySequence<byte> content)
            where TMethod : struct, IOutgoingMethod
        {
            await this.frameSender.SendMethodAsync(request);
            await this.frameSender.SendContentHeaderAsync(properties, (ulong) content.Length);
            await this.frameSender.SendContentAsync(content);
        }

        public async Task<TMethod> WaitAsync<TMethod>(CancellationToken cancellation = default)
            where TMethod : struct, IIncomingMethod
        {
            var result = await this.waitFrameHandler.WaitAsync<TMethod>(cancellation);
            return (TMethod) result;
        }
    }
}