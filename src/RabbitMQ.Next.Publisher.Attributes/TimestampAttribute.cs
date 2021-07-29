using System;
using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class TimestampAttribute : MessageAttributeBase
    {
        public override void Apply(IMessageBuilder message)
        {
            message.Timestamp ??= DateTimeOffset.UtcNow;
        }
    }
}