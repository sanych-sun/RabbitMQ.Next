using System;
using System.Collections.Generic;
using RabbitMQ.Next.Abstractions.Auth;
using RabbitMQ.Next.Transport;
using Xunit;

namespace RabbitMQ.Next.Tests
{
    public class UriExtensionsTests
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
        [MemberData(nameof(CreateTestCases))]
        public void Create(Uri uri, string host, int port, bool ssl, string vhost, PlainAuthMechanism authMechanism)
        {
            var parsed = uri.ParseAmqpUri();

            Assert.Equal(host, parsed.host);
            Assert.Equal(port, parsed.port);
            Assert.Equal(ssl, parsed.ssl);
            Assert.Equal(vhost, parsed.vhost);
            if (authMechanism != null && parsed.authMechanism != null)
            {
                var plain = (PlainAuthMechanism) parsed.authMechanism;
                Assert.Equal(authMechanism, plain, AuthMechanismComparer.Comparer);
            }
        }

        [Fact]
        public void CreateThrowsOnNoAmqp()
        {
            Assert.Throws<NotSupportedException>(() => new Uri("http://rabbitmq.com").ParseAmqpUri());
        }

        [Theory]
        [InlineData("localhost", 12345, false, "amqp://localhost:12345")]
        [InlineData("localhost", 1122, true, "amqps://localhost:1122")]
        public void ToUriTests(string host, int port, bool ssl, string expected)
        {
            var endpoint = new Endpoint(host, port, ssl);

            var result = endpoint.ToUri();

            Assert.Equal(new Uri(expected), result);
        }

        public static IEnumerable<object[]> CreateTestCases()
        {
            yield return new object[]
            {
                new Uri("amqp://user:pass@host:10000/vhost"),
                "host", 10000, false, "vhost", new PlainAuthMechanism("user", "pass")
            };

            yield return new object[]
            {
                new Uri("amqp://user@localhost"),
                "localhost", 5672, false, ProtocolConstants.DefaultVHost, new PlainAuthMechanism("user", "")
            };

            yield return new object[]
            {
                new Uri("amqp://localhost"),
                "localhost", 5672, false, ProtocolConstants.DefaultVHost, null
            };

            yield return new object[]
            {
                new Uri("amqp://[::1]"),
                "[::1]", 5672, false, ProtocolConstants.DefaultVHost, null
            };

            yield return new object[]
            {
                new Uri("amqps://user:pass@host:10000/vhost"),
                "host", 10000, true, "vhost", new PlainAuthMechanism("user", "pass")
            };

            yield return new object[]
            {
                new Uri("amqps://user@localhost"),
                "localhost", 5671, true, ProtocolConstants.DefaultVHost, new PlainAuthMechanism("user", "")
            };

            yield return new object[]
            {
                new Uri("amqps://localhost"),
                "localhost", 5671, true, ProtocolConstants.DefaultVHost, null
            };

            yield return new object[]
            {
                new Uri("amqps://[::1]"),
                "[::1]", 5671, true, ProtocolConstants.DefaultVHost, null
            };
        }

        private class AuthMechanismComparer : IEqualityComparer<PlainAuthMechanism>
        {
            public static IEqualityComparer<PlainAuthMechanism> Comparer = new AuthMechanismComparer();


            public bool Equals(PlainAuthMechanism x, PlainAuthMechanism y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.UserName == y.UserName && x.Password == y.Password;
            }

            public int GetHashCode(PlainAuthMechanism obj)
            {
                return HashCode.Combine(obj.UserName, obj.Password);
            }
        }
    }
}