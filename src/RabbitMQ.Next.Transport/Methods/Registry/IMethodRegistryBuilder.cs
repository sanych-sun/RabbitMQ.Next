using System;

namespace RabbitMQ.Next.Transport.Methods.Registry
{
    public interface IMethodRegistryBuilder
    {
        IMethodRegistryBuilder Register<TMethod>(uint methodId, Action<IMethodRegistrationBuilder<TMethod>> registration)
            where TMethod : struct, IMethod;

    }
}