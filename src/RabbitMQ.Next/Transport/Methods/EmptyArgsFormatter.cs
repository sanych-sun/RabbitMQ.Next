using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods;

internal class EmptyArgsFormatter<TMethod> : IMethodFormatter<TMethod>
    where TMethod: struct, IOutgoingMethod
{
    public void Write(IBinaryWriter destination, TMethod method)
    {
    }
}