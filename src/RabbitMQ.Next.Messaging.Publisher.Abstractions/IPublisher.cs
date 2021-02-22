using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.MessagePublisher.Abstractions
{
    public interface IPublisher : IAsyncDisposable
    {
        ValueTask PublishAsync<TContent>(TContent content, string exchange = null, string routingKey = null, IMessageProperties properties = null, PublishFlags flags = PublishFlags.None, CancellationToken cancellationToken = default);

        IDisposable RegisterReturnedMessageHandler(ReturnedMessageDelegate returnedMessageHandler);

        ValueTask CompleteAsync();
    }

    public delegate void ReturnedMessageDelegate(IReturnedMessage message, IContent content);

}