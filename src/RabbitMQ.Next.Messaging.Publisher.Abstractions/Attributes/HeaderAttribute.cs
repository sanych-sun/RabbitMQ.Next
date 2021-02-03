using System;

namespace RabbitMQ.Next.MessagePublisher.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
    public class HeaderAttribute : Attribute
    {
        public HeaderAttribute(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; }
        
        public string Value { get; }
    }
}