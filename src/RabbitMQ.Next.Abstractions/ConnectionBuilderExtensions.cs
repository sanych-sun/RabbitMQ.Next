using System;
using System.Net;
using RabbitMQ.Next.Abstractions.Auth;

namespace RabbitMQ.Next.Abstractions
{
    public static class ConnectionBuilderExtensions
    {
        private const string DefaultSchemaName = "amqp";
        private const string SslSchemaName = "amqps";
        private const int DefaultPort = 5672;
        private const int DefaultSslPort = 5671;
        private const string DefaultVirtualHost = "/";

        public static IConnectionBuilder PlainAuth(this IConnectionBuilder builder, string user, string password)
        {
            builder.Auth(new PlainAuthMechanism(user, password));
            return builder;
        }

        public static IConnectionBuilder Endpoint(this IConnectionBuilder builder, string endpoint)
        {
            endpoint = WebUtility.UrlDecode(endpoint);
            if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
            {
                return builder.Endpoint(uri);
            }

            throw new ArgumentException("Cannot parse endpoint as Uri.",nameof(endpoint));
        }

        public static IConnectionBuilder Endpoint(this IConnectionBuilder builder, Uri endpoint)
        {
            var parsed = ParseAmqpUri(endpoint);

            builder.Endpoint(parsed.host, parsed.port, parsed.ssl);
            builder.VirtualHost(parsed.vhost);
            if (parsed.authMechanism != null)
            {
                builder.Auth(parsed.authMechanism);
            }

            return builder;
        }

        private static (string host, int port, bool ssl, string vhost, IAuthMechanism authMechanism) ParseAmqpUri(Uri endpoint)
        {
            if (!string.Equals(endpoint.Scheme, DefaultSchemaName, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(endpoint.Scheme, SslSchemaName, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(@"Endpoint scheme '{endpoint.Scheme}' does not supported.", nameof(endpoint));
            }

            var ssl = endpoint.Scheme == SslSchemaName;
            var port = endpoint.Port;
            if (endpoint.IsDefaultPort)
            {
                port = ssl ? DefaultSslPort : DefaultPort;
            }

            var vhost = endpoint.GetComponents(UriComponents.Path, UriFormat.Unescaped);
            if (string.IsNullOrEmpty(vhost))
            {
                vhost = DefaultVirtualHost;
            }

            IAuthMechanism authMechanism = null;
            if (!string.IsNullOrEmpty(endpoint.UserInfo))
            {
                string user;
                string password;

                var delimiterPosition = endpoint.UserInfo.IndexOf(':');
                if (delimiterPosition >= 0)
                {
                    user = endpoint.UserInfo.Substring(0, delimiterPosition);
                    password = endpoint.UserInfo.Substring(delimiterPosition + 1);
                }
                else
                {
                    user = endpoint.UserInfo;
                    password = string.Empty;
                }

                authMechanism = new PlainAuthMechanism(user, password);
            }

            return (endpoint.Host, port, ssl, vhost, authMechanism);
        }
    }
}