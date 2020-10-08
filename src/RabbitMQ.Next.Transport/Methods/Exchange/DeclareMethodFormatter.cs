using System;
using System.IO;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    internal class DeclareMethodFormatter : IMethodFormatter<DeclareMethod>
    {
        public Span<byte> Write(Span<byte> destination, DeclareMethod method)
        {
            var res = destination
                .Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Name)
                .Write(method.Type)
                .Write((byte) method.Flags)
                .Write(method.Arguments);

            using (var file = new FileStream("/home/sanych-sun/Programming/RabbitMQ.Next/declare.dat", FileMode.Create))
            {
                file.Write(destination.Slice(0, destination.Length - res.Length));
            }

            return res;
        }
    }
}