using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Exchange;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Exchange
{
    public class SerializationTests : SerializationTestBase
    {
        public SerializationTests()
            : base(builder => builder.AddExchangeMethods())
        {
        }

        [Fact]
        public void DeclareMethodFormatter()
            => this.TestFormatter(new DeclareMethod("myexchange", "direct", true, false, false, false,null));


        [Fact]
        public void DeclareOkMethodParser()
            => this.TestParser(new DeclareOkMethod());

        [Fact]
        public void BindMethodFormatter()
            => this.TestFormatter(new BindMethod("myexchange", "amq.direct", string.Empty, null));

        [Fact]
        public void BindOkMethodParser()
            => this.TestParser(new BindOkMethod());

        [Fact]
        public void UnbindMethodFormatter()
            => this.TestFormatter(new UnbindMethod("myexchange", "amq.direct", string.Empty, null));

        [Fact]
        public void UnbindOkMethodParser()
            => this.TestParser(new UnbindOkMethod());

        [Fact]
        public void DeleteMethodFormatter()
            => this.TestFormatter(new DeleteMethod("myexchange", true));

        [Fact]
        public void DeleteOkMethodParser()
            => this.TestParser(new DeleteOkMethod());
    }
}