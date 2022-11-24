using Xunit;

namespace RabbitMQ.Next.Tests.Transport;

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

    [Theory]
    [InlineData("amqp://localhost:5672/", false, "localhost", 5672)]
    [InlineData("amqps://rmq.cloudamqp.com:5671/", true, "rmq.cloudamqp.com", 5671)]
    public void ToUri(string expectedUri, bool ssl, string host, int port)
    {
        var endpoint = new Endpoint(host, port, ssl);
        var uri = endpoint.ToUri();

        Assert.Equal(expectedUri, uri.ToString());
    }
}