using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer
{
    internal class DefaultAcknowledgement : IAcknowledgement
    {
        private readonly ChannelWriter<(ulong DeliveryTag, bool? Requeue)> ackItems;
        private readonly Task ackTask;

        public DefaultAcknowledgement(IChannel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            var ackItemsCh = Channel.CreateUnbounded<(ulong, bool?)>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
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

            return this.AckInternalAsync(deliveryTag);
        }

        public ValueTask NackAsync(ulong deliveryTag, bool requeue)
        {
            if (this.ackItems.TryWrite((deliveryTag, requeue)))
            {
                return default;
            }

            return this.NackInternalAsync(deliveryTag, requeue);
        }

        private async ValueTask AckInternalAsync(ulong deliveryTag)
        {
            try
            {
                await this.ackItems.WriteAsync((deliveryTag, null));
            }
            catch (ChannelClosedException)
            {
                throw new ObjectDisposedException(nameof(DefaultAcknowledgement));
            }
        }

        private async ValueTask NackInternalAsync(ulong deliveryTag, bool requeue)
        {
            try
            {
                await this.ackItems.WriteAsync((deliveryTag, requeue));
            }
            catch (ChannelClosedException)
            {
                throw new ObjectDisposedException(nameof(DefaultAcknowledgement));
            }
        }

        private static async Task AckLoop(IChannel channel, ChannelReader<(ulong DeliveryTag, bool? Requeue)> ackItemsReader)
        {
            while (await ackItemsReader.WaitToReadAsync())
            {
                while (ackItemsReader.TryRead(out var ack))
                {
                    if (ack.Requeue.HasValue)
                    {
                        await channel.SendAsync(new NackMethod(ack.DeliveryTag, false, ack.Requeue.Value));
                    }
                    else
                    {
                        await channel.SendAsync(new AckMethod(ack.DeliveryTag, false));
                    }
                }

            }
        }
    }
}