using System;

namespace RabbitMQ.Next.Publisher.Initializers;

public class ContentTypeInitializer : IMessageInitializer
{
    private readonly string contentType;

    public ContentTypeInitializer(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentNullException(nameof(contentType));
        }

        this.contentType = contentType;
    }

    public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
        => message.ContentType(this.contentType);
}