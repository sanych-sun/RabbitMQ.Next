using System;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;

namespace RabbitMQ.Next.Publisher.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
    public class HeaderAttribute : MessageAttributeBase
    {
        public HeaderAttribute(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; }
        
        public string Value { get; }

        public override void Apply(IMessageBuilder message)
        {
            if (message.Headers == null || !message.Headers.TryGetValue(this.Name, out var _))
            {
                message.SetHeader(this.Name, this.Value);
            }
        }
    }
}