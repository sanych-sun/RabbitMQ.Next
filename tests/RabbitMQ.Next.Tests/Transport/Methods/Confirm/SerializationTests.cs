using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Confirm;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Confirm
{
    public class SerializationTests: SerializationTestBase
    {
        public SerializationTests()
            : base(builder => builder.AddConfirmMethods())
        {
        }

        [Fact]
        public void SelectMethodFormatter()
            => this.TestFormatter(new SelectMethod(true));

        [Fact]
        public void SelectOkMethodParser()
            => this.TestParser(new SelectOkMethod());
    }
}