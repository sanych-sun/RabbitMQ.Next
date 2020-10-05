using System;

namespace RabbitMQ.Next.Transport.Methods
{
    public interface IMethodFrameParser
    {
        IMethod ParseMethod(ReadOnlySpan<byte> payload);
    }

    public interface IMethodFrameParser<out TMethod> : IMethodFrameParser
        where TMethod : struct, IMethod
    {
        TMethod Parse(ReadOnlySpan<byte> payload);
    }
}