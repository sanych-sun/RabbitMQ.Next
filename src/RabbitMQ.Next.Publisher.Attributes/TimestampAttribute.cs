using System;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;

namespace RabbitMQ.Next.Publisher.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class TimestampAttribute : MessageAttributeBase
    {
        public override void Apply(IMessageBuilder message)
        {
            if (!message.Timestamp.HasValue)
            {
                message.SetTimestamp(DateTimeOffset.UtcNow);
            }
        }
    }
}