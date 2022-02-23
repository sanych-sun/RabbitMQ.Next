namespace RabbitMQ.Next.Methods
{
    public interface IMethodRegistrationBuilder<TMethod>
        where TMethod : struct, IMethod
    {
        IMethodRegistrationBuilder<TMethod> HasContent();

        IMethodRegistrationBuilder<TMethod> Use(IMethodParser<TMethod> parser);

        IMethodRegistrationBuilder<TMethod> Use(IMethodFormatter<TMethod> formatter);
    }
}