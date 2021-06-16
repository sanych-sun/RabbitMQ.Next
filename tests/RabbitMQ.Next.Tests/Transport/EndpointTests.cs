using RabbitMQ.Next.Transport;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport
{
    public class EndpointTests
    {
        [Fact]
        public void EndpointCtor()
        {
            var host = "testhost";
            var port = 12345;
            var ssl = true;

            var endpoint = new Endpoint(host, port, ssl);

            Assert.Equal(host, endpoint.Host);
            Assert.Equal(port, endpoint.Port);
            Assert.Equal(ssl, endpoint.UseSsl);
        }
    }
}