using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Registry
{
    internal class MethodRegistration<TMethod> : IMethodRegistrationBuilder<TMethod>, IMethodRegistration
        where TMethod: struct, IMethod
    {
        public MethodRegistration(MethodId methodId)
        {
            this.MethodId = methodId;
            this.Type = typeof(TMethod);
        }

        public MethodId MethodId { get; }

        public Type Type { get; }

        public bool HasContent { get; private set; }

        public object Parser { get; private set; }

        public object Formatter { get; private set; }

        IMethodRegistrationBuilder<TMethod> IMethodRegistrationBuilder<TMethod>.HasContent()
        {
            this.HasContent = true;
            return this;
        }

        IMethodRegistrationBuilder<TMethod> IMethodRegistrationBuilder<TMethod>.Use(IMethodParser<TMethod> parser)
        {
            this.Parser = parser;
            return this;
        }

        IMethodRegistrationBuilder<TMethod> IMethodRegistrationBuilder<TMethod>.Use(IMethodFormatter<TMethod> formatter)
        {
            this.Formatter = formatter;
            return this;
        }
    }
}