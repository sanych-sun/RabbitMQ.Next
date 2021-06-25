using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Transport.Methods.Basic;
using RabbitMQ.Next.Transport.Methods.Channel;

namespace RabbitMQ.Next.Consumer
{
    internal class ConsumerInitializer
    {
        public ConsumerInitializer(IReadOnlyList<QueueConsumerBuilder> queues, uint prefetchSize, ushort prefetchCount, bool noAck)
        {
            this.Queues = queues;
            this.PrefetchSize = prefetchSize;
            this.PrefetchCount = prefetchCount;
            this.NoAck = noAck;
        }

        public readonly IReadOnlyList<QueueConsumerBuilder> Queues;

        public uint PrefetchSize { get; }

        public ushort PrefetchCount { get; }

        public bool NoAck { get; }

        public async Task InitConsumerAsync(ISynchronizedChannel channel, CancellationToken cancellation)
        {
            await channel.SendAsync<FlowMethod, FlowOkMethod>(new FlowMethod(false), cancellation);
            await channel.SendAsync<QosMethod, QosOkMethod>(new QosMethod(this.PrefetchSize, this.PrefetchCount, false), cancellation);

            for (var i = 0; i < this.Queues.Count; i++)
            {
                var queue = this.Queues[i];
                var response = await channel.SendAsync<ConsumeMethod, ConsumeOkMethod>(
                    new ConsumeMethod(
                        queue.Queue, queue.ConsumerTag, queue.NoLocal, this.NoAck,
                        queue.Exclusive, false, queue.Arguments), cancellation);

                queue.ConsumerTag = response.ConsumerTag;
            }

            await channel.SendAsync<FlowMethod, FlowOkMethod>(new FlowMethod(true), cancellation);
        }

        public async Task CancelAsync(ISynchronizedChannel channel)
        {
            for (var i = 0; i < this.Queues.Count; i++)
            {
                var queue = this.Queues[i];
                await channel.SendAsync<CancelMethod, CancelOkMethod>(new CancelMethod(queue.ConsumerTag, false));
            }
        }
    }
}