using System.Threading.Channels;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer
{
    internal class DefaultAcknowledgement : IAcknowledgement
    {
        private readonly ChannelWriter<(ulong DeliveryTag, bool? Requeue)> ackItems;
        private readonly Task ackTask;

        public DefaultAcknowledgement(IChannel channel)
        {
            var ackItemsCh = Channel.CreateUnbounded<(ulong, bool?)>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true,
                AllowSynchronousContinuations = false
            });

            this.ackItems = ackItemsCh.Writer;
            this.ackTask = Task.Factory.StartNew(() => AckLoop(channel, ackItemsCh.Reader), TaskCreationOptions.LongRunning);
        }

        public ValueTask DisposeAsync()
        {
            this.ackItems.TryComplete();
            return new ValueTask(this.ackTask);
        }

        public ValueTask AckAsync(ulong deliveryTag)
        {
            if (this.ackItems.TryWrite((deliveryTag, null)))
            {
                return default;
            }

            return this.ackItems.WriteAsync((deliveryTag, null));
        }

        public ValueTask NackAsync(ulong deliveryTag, bool requeue)
        {
            if (this.ackItems.TryWrite((deliveryTag, requeue)))
            {
                return default;
            }

            return this.ackItems.WriteAsync((deliveryTag, requeue));
        }

        private static async Task AckLoop(IChannel channel, ChannelReader<(ulong DeliveryTag, bool? Requeue)> ackItemsReader)
        {
            while (await ackItemsReader.WaitToReadAsync())
            {
                ulong deliveryTag = 0;

                while (ackItemsReader.TryRead(out var ack))
                {
                    if (ack.Requeue.HasValue)
                    {
                        await channel.SendAsync(new NackMethod(ack.DeliveryTag, false, ack.Requeue.Value));
                    }
                    else
                    {
                        deliveryTag = ack.DeliveryTag;
                    }
                }

                if (deliveryTag != 0)
                {
                    await channel.SendAsync(new AckMethod(deliveryTag, true));
                }
            }
        }
    }
}