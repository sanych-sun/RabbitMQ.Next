using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Registry
{
    public interface IMethodRegistrationBuilder<TMethod>
        where TMethod : struct, IMethod
    {
        IMethodRegistrationBuilder<TMethod> HasContent();

        IMethodRegistrationBuilder<TMethod> Use(IMethodParser<TMethod> parser);

        IMethodRegistrationBuilder<TMethod> Use(IMethodFormatter<TMethod> formatter);
    }
}