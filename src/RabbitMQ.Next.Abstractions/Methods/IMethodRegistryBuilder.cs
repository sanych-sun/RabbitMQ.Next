using System;

namespace RabbitMQ.Next.Abstractions.Methods
{
    public interface IMethodRegistryBuilder
    {
        IMethodRegistryBuilder Register<TMethod>(MethodId methodId, Action<IMethodRegistrationBuilder<TMethod>> registration)
            where TMethod : struct, IMethod;

        IMethodRegistry Build();
    }
}