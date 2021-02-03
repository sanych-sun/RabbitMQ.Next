using System;

namespace RabbitMQ.Next.MessagePublisher.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TypeAttribute : Attribute
    {
        public TypeAttribute(string type)
        {
            this.Type = type;
        }

        public string Type { get; }
    }
}