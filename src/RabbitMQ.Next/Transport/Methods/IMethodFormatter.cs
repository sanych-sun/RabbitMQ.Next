using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods;

internal interface IMethodFormatter<in TMethod>
    where TMethod : struct, IMethod
{
    void Write(IBufferBuilder destination, TMethod method);
}