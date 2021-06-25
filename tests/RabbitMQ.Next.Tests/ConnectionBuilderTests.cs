using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport;
using Xunit;

namespace RabbitMQ.Next.Tests
{
    public class ConnectionBuilderTests
    {
        [Fact]
        public async Task Auth()
        {
            var factory = Substitute.For<IConnectionFactory>();
            var builder = new ConnectionBuilder(factory);
            var auth = Substitute.For<IAuthMechanism>();

            builder.Auth(auth);

            await builder.ConnectAsync();

            await factory.Received().ConnectAsync(
                Arg.Any<IReadOnlyList<Endpoint>>(), Arg.Any<string>(), auth,
                Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, object>>(), Arg.Any<IMethodRegistry>());
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
                Arg.Any<IReadOnlyList<Endpoint>>(), virtualHost, Arg.Any<IAuthMechanism>(),
                Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, object>>(), Arg.Any<IMethodRegistry>());
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
                Arg.Is<IReadOnlyList<Endpoint>>(x => endpoints.All(i => x.Count(e => e.Host == i.host && e.Port == i.port && e.UseSsl == i.ssl) == 1)),
                Arg.Any<string>(), Arg.Any<IAuthMechanism>(),
                Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, object>>(), Arg.Any<IMethodRegistry>());
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
                Arg.Any<IReadOnlyList<Endpoint>>(), Arg.Any<string>(), Arg.Any<IAuthMechanism>(),
                locale, Arg.Any<IReadOnlyDictionary<string, object>>(), Arg.Any<IMethodRegistry>());
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
                Arg.Any<IReadOnlyList<Endpoint>>(), Arg.Any<string>(), Arg.Any<IAuthMechanism>(),
                Arg.Any<string>(),
                Arg.Is<IReadOnlyDictionary<string, object>>(x => properties.All(p => x.Count(i => p.key == i.Key && p.value == i.Value) == 1)),
                Arg.Any<IMethodRegistry>());
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