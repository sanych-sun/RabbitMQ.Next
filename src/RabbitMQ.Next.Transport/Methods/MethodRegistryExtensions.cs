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
                // TODO: throw connection-level exception here?
                throw new InvalidOperationException();
            }

            var resultSpan = buffer.Span
                .Write(method.Method);

            resultSpan = formatter.Write(resultSpan, method);

            return (buffer.Length - resultSpan.Length);
        }
    }
}