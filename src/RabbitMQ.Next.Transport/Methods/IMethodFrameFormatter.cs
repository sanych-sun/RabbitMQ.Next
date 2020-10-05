using System;

namespace RabbitMQ.Next.Transport.Methods
{
    public interface IMethodFrameFormatter<in TMethod>
        where TMethod : struct, IMethod
    {
        Span<byte> Write(Span<byte> destination, TMethod method);
    }
}