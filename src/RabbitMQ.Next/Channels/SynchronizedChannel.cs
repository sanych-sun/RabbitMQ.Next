using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Channels
{
    internal class SynchronizedChannel : ISynchronizedChannel
    {
        private readonly ushort channelNumber;
        private readonly IFrameSender frameSender;
        private readonly WaitMethodHandler waitHandler;

        public SynchronizedChannel(ushort channelNumber, IFrameSender frameSender, WaitMethodHandler waitHandler)
        {
            this.channelNumber = channelNumber;
            this.frameSender = frameSender;
            this.waitHandler = waitHandler;
        }

        public async Task SendAsync<TMethod>(TMethod request)
            where TMethod : struct, IOutgoingMethod
            => await this.frameSender.SendMethodAsync(this.channelNumber, request);

        public async Task SendAsync<TMethod>(TMethod request, IMessageProperties properties, ReadOnlySequence<byte> content)
            where TMethod : struct, IOutgoingMethod
        {
            await this.frameSender.SendMethodAsync(this.channelNumber, request);
            await this.frameSender.SendContentHeaderAsync(this.channelNumber, properties, (ulong)content.Length);
            if (!content.IsEmpty)
            {
                await this.frameSender.SendContentAsync(this.channelNumber, content);
            }
        }

        public async Task<TMethod> WaitAsync<TMethod>(CancellationToken cancellation = default)
            where TMethod : struct, IIncomingMethod
        {
            var result = await this.waitHandler.WaitAsync<TMethod>(cancellation);
            return (TMethod) result;
        }
    }
}