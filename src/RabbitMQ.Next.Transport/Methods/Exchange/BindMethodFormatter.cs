using System;
using System.IO;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    internal class BindMethodFormatter : IMethodFormatter<BindMethod>
    {
        public Span<byte> Write(Span<byte> destination, BindMethod method)
        {
            var res = destination.Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Destination)
                .Write(method.Source)
                .Write(method.RoutingKey)
                .Write(false)
                .Write(method.Arguments);

            using (var file = new FileStream("/home/sanych-sun/Programming/RabbitMQ.Next/bind.dat", FileMode.Create))
            {
                file.Write(destination.Slice(0, destination.Length - res.Length));
            }

            return res;
        }
    }
}