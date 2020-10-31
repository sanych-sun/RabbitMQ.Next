using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Basic;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Basic
{
    public class SerializationTests : SerializationTestBase
    {
        public SerializationTests()
            : base(builder => builder.AddBasicMethods())
        {
        }

        [Fact]
        public void PublishMethodFormatter()
            => this.TestFormatter(new PublishMethod("exchange", "routingKey", false, false));

    }
}