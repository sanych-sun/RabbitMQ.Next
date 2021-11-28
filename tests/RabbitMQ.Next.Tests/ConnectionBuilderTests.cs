using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;
using Xunit;

namespace RabbitMQ.Next.Tests
{
    public class ConnectionBuilderTests
    {
        [Fact]
        public async Task DefaultValues()
        {
            var factory = Substitute.For<IConnectionFactory>();
            var builder = new ConnectionBuilder(factory);

            await builder.ConnectAsync();

            await factory.Received().ConnectAsync(
                Arg.Is<ConnectionSettings>(c => c.Locale == "en-US" && c.MaxFrameSize == 131_072),
                Arg.Any<IMethodRegistry>(),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ConnectPassCancellation()
        {
            var factory = Substitute.For<IConnectionFactory>();
            var builder = new ConnectionBuilder(factory);
            var cancellation = new CancellationTokenSource().Token;

            await builder.ConnectAsync(cancellation);

            await factory.Received().ConnectAsync(
                Arg.Any<ConnectionSettings>(),
                Arg.Any<IMethodRegistry>(),
                cancellation);
        }

        [Fact]
        public async Task Auth()
        {
            var factory = Substitute.For<IConnectionFactory>();
            var builder = new ConnectionBuilder(factory);
            var auth = Substitute.For<IAuthMechanism>();

            builder.Auth(auth);

            await builder.ConnectAsync();

            await factory.Received().ConnectAsync(
                Arg.Is<ConnectionSettings>(c => c.Auth == auth),
                Arg.Any<IMethodRegistry>(),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task VirtualHost()
        {
            var factory = Substitute.For<IConnectionFactory>();
            var builder = new ConnectionBuilder(factory);
            var virtualHost = "vhost";

            builder.VirtualHost(virtualHost);

            await builder.ConnectAsync();

            await factory.Received().ConnectAsync(
                Arg.Is<ConnectionSettings>(c => c.Vhost == virtualHost),
                Arg.Any<IMethodRegistry>(),
                Arg.Any<CancellationToken>());
        }

        [Theory]
        [MemberData(nameof(AddEndpointTestCases))]
        public async Task AddEndpoint(IEnumerable<(string host, int port, bool ssl)> endpoints)
        {
            var factory = Substitute.For<IConnectionFactory>();
            var builder = new ConnectionBuilder(factory);

            foreach (var endpoint in endpoints)
            {
                builder.AddEndpoint(endpoint.host, endpoint.port, endpoint.ssl);
            }

            await builder.ConnectAsync();

            await factory.Received().ConnectAsync(
                Arg.Is<ConnectionSettings>(c => endpoints.All(e => c.Endpoints.Any(e1 => e1.Host == e.host && e1.Port == e.port && e1.UseSsl == e.ssl))),
                Arg.Any<IMethodRegistry>(),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Locale()
        {
            var factory = Substitute.For<IConnectionFactory>();
            var builder = new ConnectionBuilder(factory);
            var locale = "uk-ua";

            builder.Locale(locale);

            await builder.ConnectAsync();

            await factory.Received().ConnectAsync(
                Arg.Is<ConnectionSettings>(c => c.Locale == locale),
                Arg.Any<IMethodRegistry>(),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task MaxFrameSize()
        {
            var factory = Substitute.For<IConnectionFactory>();
            var builder = new ConnectionBuilder(factory);
            var maxFrameSize = 12345;

            builder.MaxFrameSize(maxFrameSize);

            await builder.ConnectAsync();

            await factory.Received().ConnectAsync(
                Arg.Is<ConnectionSettings>(c => c.MaxFrameSize == maxFrameSize),
                Arg.Any<IMethodRegistry>(),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public void MaxFrameSize_ThrowsOnInvalidValue()
        {
            var factory = Substitute.For<IConnectionFactory>();
            var builder = new ConnectionBuilder(factory);

            Assert.Throws<ArgumentOutOfRangeException>(() => builder.MaxFrameSize(100));
        }

        [Theory]
        [MemberData(nameof(ClientPropertyTestCases))]
        public async Task ClientProperty(IEnumerable<(string key, object value)> properties)
        {
            var factory = Substitute.For<IConnectionFactory>();
            var builder = new ConnectionBuilder(factory);

            foreach (var prop in properties)
            {
                builder.ClientProperty(prop.key, prop.value);
            }

            await builder.ConnectAsync();

            await factory.Received().ConnectAsync(
                Arg.Is<ConnectionSettings>(x => properties.All(p => x.ClientProperties.Any(i => p.key == i.Key && p.value == i.Value))),
                Arg.Any<IMethodRegistry>(),
                Arg.Any<CancellationToken>());
        }

        public static IEnumerable<object[]> AddEndpointTestCases()
        {
            yield return new object[] { new[] { ( "host", 123, true )}};

            yield return new object[] { new[] { ( "127.0.0.1", 321, false )}};

            yield return new object[] { new[] { ( "host", 123, true ), ( "127.0.0.1", 321, false )}};
        }

        public static IEnumerable<object[]> ClientPropertyTestCases()
        {
            yield return new object[] { new (string, object)[] { ( "test", "value" )}};

            yield return new object[] { new (string, object)[] { ( "key", 123 )}};

            yield return new object[] { new (string, object)[] { ("test", "value"), ("key", 123)}};
        }
    }
}