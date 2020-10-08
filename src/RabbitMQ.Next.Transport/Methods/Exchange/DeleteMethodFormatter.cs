using System;
using System.IO;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    public class DeleteMethodFormatter : IMethodFormatter<DeleteMethod>
    {
        public Span<byte> Write(Span<byte> destination, DeleteMethod method)
        {
            var res = destination.Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Name)
                .Write(method.UnusedOnly);

            using (var file = new FileStream("/home/sanych-sun/Programming/RabbitMQ.Next/delete.dat", FileMode.Create))
            {
                file.Write(destination.Slice(0, destination.Length - res.Length));
            }

            return res;
        }
    }
}