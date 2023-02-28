using System;
using System.Globalization;

namespace RabbitMQ.Next.Publisher.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
public class ExpirationAttribute : MessageAttributeBase
{
    private readonly string expirationText;

    public ExpirationAttribute(int seconds)
    {
        if (seconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds));
        }

        this.Expiration = TimeSpan.FromSeconds(seconds);
        this.expirationText = this.Expiration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
    }

    public TimeSpan Expiration { get; }

    public override void Apply(IMessageBuilder message)
        => message.SetExpiration(this.expirationText);
}