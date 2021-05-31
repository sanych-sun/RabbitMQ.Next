using System;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Registry
{
    public interface IMethodRegistryBuilder
    {
        IMethodRegistryBuilder Register<TMethod>(MethodId methodId, Action<IMethodRegistrationBuilder<TMethod>> registration)
            where TMethod : struct, IMethod;

        IMethodRegistry Build();
    }
}