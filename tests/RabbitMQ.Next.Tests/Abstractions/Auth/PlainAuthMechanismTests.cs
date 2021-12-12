using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Auth;
using Xunit;

namespace RabbitMQ.Next.Tests.Abstractions.Auth
{
    public class PlainAuthMechanismTests
    {
        [Fact]
        public void CtorTests()
        {
            var user = "test";
            var password = "pwd";

            var auth = new PlainAuthMechanism(user, password);

            Assert.Equal("PLAIN", auth.Type);
            Assert.Equal(user, auth.UserName);
            Assert.Equal(password, auth.Password);
        }

        [Fact]
        public void ToResponse()
        {
            var user = "test";
            var password = "pwd";

            var auth = new PlainAuthMechanism(user, password);

            Assert.Equal($"\0{user}\0{password}", auth.ToResponse());
        }

        [Fact]
        public void ExtensionTests()
        {
            var user = "test2";
            var password = "pwd2";

            var builder = Substitute.For<IConnectionBuilder>();
            builder.PlainAuth(user, password);

            builder.Received().Auth(Arg.Is<IAuthMechanism>(a => ((PlainAuthMechanism)a).UserName == user && ((PlainAuthMechanism)a).Password == password)
            );
        }
    }
}