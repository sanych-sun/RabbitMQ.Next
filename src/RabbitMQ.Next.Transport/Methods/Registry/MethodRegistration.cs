using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Registry
{
    internal class MethodRegistration<TMethod> : IMethodRegistrationBuilder<TMethod>, IMethodRegistration
        where TMethod: struct, IMethod
    {
        public MethodRegistration(uint methodId)
        {
            this.MethodId = methodId;
            this.ImplementationType = typeof(TMethod);
        }

        public uint MethodId { get; }

        public Type ImplementationType { get; }

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