using System;
using System.Collections.Generic;
using RabbitMQ.Next.Transport;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport
{
    public class ConnectionStringTests
    {
        [Fact]
        public void EndpointCtor()
        {
            var host = "testhost";
            var port = 12345;

            var endpoint = new Endpoint(host, port);

            Assert.Equal(host, endpoint.Host);
            Assert.Equal(port, endpoint.Port);
        }

        [Fact]
        public void ConnectionStringCtor()
        {
            var endpoints = new[] {new Endpoint("localhost", 1234)};
            var userName = "test";
            var password = "password";
            var vHost = "/v";
            var ssl = true;

            var connection = new ConnectionString(ssl, endpoints, userName, password, vHost);

            Assert.Equal(ssl, connection.Ssl);
            Assert.Equal(endpoints, connection.EndPoints);
            Assert.Equal(userName, connection.UserName);
            Assert.Equal(password, connection.Password);
            Assert.Equal(vHost, connection.VirtualHost);
        }


        [Theory]
        [MemberData(nameof(CreateTestCases))]
        public void Create(string uri, ConnectionString expected)
        {
            var parsed = ConnectionString.Create(uri);

            Assert.Equal(expected.Ssl, parsed.Ssl);
            Assert.Equal(expected.EndPoints, parsed.EndPoints);
            Assert.Equal(expected.UserName, parsed.UserName);
            Assert.Equal(expected.Password, parsed.Password);
            Assert.Equal(expected.VirtualHost, parsed.VirtualHost);
        }

        [Fact]
        public void CreateThrowsOnNoAmqp()
        {
            Assert.Throws<NotSupportedException>(() => ConnectionString.Create("http://rabbitmq.com"));
        }

        public static IEnumerable<object[]> CreateTestCases()
        {
            yield return new object[] {"amqp://user:pass@host:10000/vhost",
                new ConnectionString(false, new[] {new Endpoint("host", 10000)}, "user", "pass", "vhost")};

            yield return new object[] {"amqp://user%61:%61pass@ho%61st:10000/v%2fhost",
                new ConnectionString(false, new[] {new Endpoint("hoast", 10000)}, "usera", "apass", "v/host")};

            yield return new object[] {"amqp://user@localhost",
                new ConnectionString(false, new[] {new Endpoint("localhost", 5672)}, "user", "")};

            yield return new object[] {"amqp://[::1]",
                new ConnectionString(false, new[] {new Endpoint("[::1]", 5672)}, "guest", "guest")};

            yield return new object[] {"amqps://user:pass@host:10000/vhost",
                new ConnectionString(true, new[] {new Endpoint("host", 10000)}, "user", "pass", "vhost")};

            yield return new object[] {"amqps://user%61:%61pass@ho%61st:10000/v%2fhost",
                new ConnectionString(true, new[] {new Endpoint("hoast", 10000)}, "usera", "apass", "v/host")};

            yield return new object[] {"amqps://user@localhost",
                new ConnectionString(true, new[] {new Endpoint("localhost", 5671)}, "user", "")};

            yield return new object[] {"amqps://[::1]",
                new ConnectionString(true, new[] {new Endpoint("[::1]", 5671)}, "guest", "guest")};
        }
    }
}