using System;
using System.IO;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    public class UnbindMethodFormatter : IMethodFormatter<UnbindMethod>
    {
        public Span<byte> Write(Span<byte> destination, UnbindMethod method)
        {
            var res = destination.Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Destination)
                .Write(method.Source)
                .Write(method.RoutingKey)
                .Write(false)
                .Write(method.Arguments);

            using (var file = new FileStream("/home/sanych-sun/Programming/RabbitMQ.Next/unbind.dat", FileMode.Create))
            {
                file.Write(destination.Slice(0, destination.Length - res.Length));
            }

            return res;
        }
    }
}