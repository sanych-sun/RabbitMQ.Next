using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods
{
    public static class MethodRegistryExtensions
    {
        public static int FormatMessage<TMethod>(this IMethodRegistry methodRegistry, TMethod method, Memory<byte> buffer)
            where TMethod : struct, IOutgoingMethod
        {
            var formatter = methodRegistry.GetFormatter<TMethod>();
            if (formatter == null)
            {
                throw new NotSupportedException();
            }

            var resultSpan = buffer.Span
                .Write((uint)method.MethodId);

            resultSpan = formatter.Write(resultSpan, method);

            return (buffer.Length - resultSpan.Length);
        }
    }
}