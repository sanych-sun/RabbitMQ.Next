using System;
using System.IO;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    internal class StartOkMethodFormatter : IMethodFormatter<StartOkMethod>
    {
        public Span<byte> Write(Span<byte> destination, StartOkMethod method)
        {
            var res = destination
                .Write(method.ClientProperties)
                .Write(method.Mechanism)
                .Write(method.Response, true)
                .Write(method.Locale);

            using (var writer = new BinaryWriter(File.Open(@"/home/sanych-sun/Programming/RabbitMQ.Next/method.bin", FileMode.Create)))
            {
                writer.Write(destination.Slice(0, destination.Length - res.Length));
            }

            return res;
        }
    }
}