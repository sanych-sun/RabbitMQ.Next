using System;
using System.Globalization;

namespace RabbitMQ.Next.Publisher.Initializers;

public class ExpirationInitializer : IMessageInitializer
{
    private readonly string expiration;

    public ExpirationInitializer(TimeSpan expiration)
    {
        if (expiration.TotalMilliseconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(expiration));
        }

        this.expiration = expiration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
    }

    public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
        => message.Expiration(this.expiration);
}