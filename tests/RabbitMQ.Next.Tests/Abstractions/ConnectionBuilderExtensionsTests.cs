using System;
using System.Collections.Generic;
using NSubstitute;
using RabbitMQ.Next.Auth;
using Xunit;

namespace RabbitMQ.Next.Tests.Abstractions
{
    public class ConnectionBuilderExtensionsTests
    {
        [Fact]
        public void AuthPlain()
        {
            var user = "test1";
            var password = "pwd";
            var builder = Substitute.For<IConnectionBuilder>();

            builder.PlainAuth(user, password);

            builder.Received().Auth(Arg.Is<PlainAuthMechanism>(a => a.UserName == user && a.Password == password));
        }

        [Fact]
        public void AddEndpointStringThrowsOnNonUri()
        {
            var builder = Substitute.For<IConnectionBuilder>();

            Assert.Throws<ArgumentException>(() => builder.Endpoint("some random text"));
        }

        [Fact]
        public void AddEndpointStringThrowsOnNoAmqp()
        {
            var builder = Substitute.For<IConnectionBuilder>();

            Assert.Throws<ArgumentException>(() => builder.Endpoint("http://rabbitmq.com"));
        }

        [Fact]
        public void AddEndpointUriThrowsOnNoAmqp()
        {
            var builder = Substitute.For<IConnectionBuilder>();

            Assert.Throws<ArgumentException>(() => builder.Endpoint(new Uri("http://rabbitmq.com")));
        }

        [Theory]
        [MemberData(nameof(AddEndpointTestCases))]
        public void AddEndpointCanParseValidUri(string endpoint, bool ssl, string host, int port, string vhost, PlainAuthMechanism auth)
        {
            var builder = Substitute.For<IConnectionBuilder>();

            builder.Endpoint(endpoint);

            builder.Received().Endpoint(host, port, ssl);
            builder.Received().VirtualHost(vhost);
            if (auth == null)
            {
                builder.DidNotReceive().Auth(Arg.Any<IAuthMechanism>());
            }
            else
            {
                builder.Received().Auth(Arg.Is<PlainAuthMechanism>(x => x.Type == "PLAIN" && x.UserName == auth.UserName && x.Password == auth.Password));
            }
        }

        public static IEnumerable<object[]> AddEndpointTestCases()
        {
            yield return new object[] {"amqp://user:pass@host:10000/vhost",
                false, "host", 10000, "vhost", new PlainAuthMechanism("user", "pass")};

            yield return new object[] {"AMQP://user:pass@host:10000/vhost",
                false, "host", 10000, "vhost", new PlainAuthMechanism("user", "pass")};

            yield return new object[] {"amqp://user%61:%61pass@ho%61st:10000/v%2fhost",
                false, "hoast", 10000, "v/host", new PlainAuthMechanism("usera", "apass")};

            yield return new object[] {"amqp://user@localhost",
                false, "localhost", 5672, "/", new PlainAuthMechanism("user", "") };

            yield return new object[] {"amqp://[::1]",
                false, "[::1]", 5672, "/", null};

            yield return new object[] {"amqps://user:pass@host:10000/vhost",
                true, "host", 10000, "vhost", new PlainAuthMechanism("user", "pass")};

            yield return new object[] {"amqps://user%61:%61pass@ho%61st:10000/v%2fhost",
                true, "hoast", 10000, "v/host", new PlainAuthMechanism("usera", "apass")};

            yield return new object[] {"amqps://user@localhost",
                true, "localhost", 5671, "/", new PlainAuthMechanism("user", "")};

            yield return new object[] {"amqps://[::1]",
                true, "[::1]", 5671, "/", null};
        }
    }
}