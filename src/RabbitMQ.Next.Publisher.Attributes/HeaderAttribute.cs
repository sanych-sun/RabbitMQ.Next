using System;
using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
    public class HeaderAttribute : MessageAttributeBase
    {
        public HeaderAttribute(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.Name = name;
            this.Value = value;
        }

        public string Name { get; }
        
        public string Value { get; }

        public override void Apply(IMessageBuilder message)
            => message.SetHeader(this.Name, this.Value);
    }
}