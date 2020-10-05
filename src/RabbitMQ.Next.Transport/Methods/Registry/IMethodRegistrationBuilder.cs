namespace RabbitMQ.Next.Transport.Methods.Registry
{
    public interface IMethodRegistrationBuilder<TMethod>
        where TMethod : struct, IMethod
    {
        IMethodRegistrationBuilder<TMethod> HasContent();

        IMethodRegistrationBuilder<TMethod> Use(IMethodFrameParser<TMethod> parser);

        IMethodRegistrationBuilder<TMethod> Use(IMethodFrameFormatter<TMethod> formatter);
    }
}