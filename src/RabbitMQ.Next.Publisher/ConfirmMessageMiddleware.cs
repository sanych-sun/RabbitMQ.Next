using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Transport.Methods.Basic;
using RabbitMQ.Next.Transport.Methods.Confirm;

namespace RabbitMQ.Next.Publisher;

internal class ConfirmMessageMiddleware : IPublishMiddleware
{
    private readonly IPublishMiddleware next;
    private ConfirmMessageHandler confirms;

    public ConfirmMessageMiddleware(IPublishMiddleware next)
    {
        this.next = next;
    }

    public ValueTask InitAsync(IChannel channel, CancellationToken cancellation)
    {
        this.confirms = new ConfirmMessageHandler();
        channel.WithMessageHandler<AckMethod>(this.confirms);
        channel.WithMessageHandler<NackMethod>(this.confirms);
    
        return new ValueTask(channel.SendAsync<SelectMethod, SelectOkMethod>(new SelectMethod(), cancellation));
    }

    public async ValueTask<ulong> InvokeAsync<TContent>(TContent content, IMessageBuilder message, CancellationToken cancellation)
    {
        var deliveryTag = await this.next.InvokeAsync(content, message, cancellation).ConfigureAwait(false);

        var confirmed = await this.confirms.WaitForConfirmAsync(deliveryTag, cancellation).ConfigureAwait(false);
        if (!confirmed)
        {
            // todo: provide some useful info here
            throw new DeliveryFailedException();
        }

        return deliveryTag;
    }
}