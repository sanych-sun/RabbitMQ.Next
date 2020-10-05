using System;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    internal class StartOkMethodFormatter : IMethodFormatter<StartOkMethod>
    {
        public Span<byte> Write(Span<byte> destination, StartOkMethod method) =>
            destination
                .Write(method.ClientProperties)
                .Write(method.Mechanism)
                .Write(method.Response, true)
                .Write(method.Locale);
    }
}