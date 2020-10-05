using System;
using System.IO;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    public class CloseMethodFrameFormatter : IMethodFrameFormatter<CloseMethod>
    {
        public Span<byte> Write(Span<byte> destination, CloseMethod method) =>
            destination.Write((ushort) method.StatusCode)
                .Write(method.Description)
                .Write(method.FailedMethodUid);
    }
}