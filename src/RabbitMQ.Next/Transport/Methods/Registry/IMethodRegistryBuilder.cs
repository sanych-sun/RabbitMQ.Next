using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Registry;

internal interface IMethodRegistryBuilder
{
    IMethodRegistryBuilder Register<TMethod>(MethodId methodId, Action<IMethodRegistrationBuilder<TMethod>> registration)
        where TMethod : struct, IMethod;

    IMethodRegistry Build();
}