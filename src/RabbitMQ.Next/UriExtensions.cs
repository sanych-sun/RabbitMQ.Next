using System;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Auth;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next
{
    internal static class UriExtensions
    {
        private const string DefaultSchemaName = "amqp";
        private const string SslSchemaName = "amqps";
        private const int DefaultPort = 5672;
        private const int SslDefaultPort = 5671;

        public static Uri ToUri(this Endpoint endpoint)
            => new Uri($"amqp{(endpoint.UseSsl ? "s" : "")}://{endpoint.Host}:{endpoint.Port}");

        public static (string host, int port, bool ssl, string vhost, IAuthMechanism authMechanism) ParseAmqpUri(this Uri uri)
        {
            if (uri.Scheme != DefaultSchemaName && uri.Scheme != SslSchemaName)
            {
                throw new NotSupportedException();
            }

            var ssl = uri.Scheme == SslSchemaName;
            var port = uri.Port;
            if (uri.IsDefaultPort)
            {
                port = ssl ? SslDefaultPort : DefaultPort;
            }

            var vhost = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
            if (string.IsNullOrEmpty(vhost))
            {
                vhost = ProtocolConstants.DefaultVHost;
            }

            IAuthMechanism authMechanism = null;
            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                string user;
                string password;

                var delimiterPosition = uri.UserInfo.IndexOf(':');
                if (delimiterPosition >= 0)
                {
                    user = uri.UserInfo.Substring(0, delimiterPosition);
                    password = uri.UserInfo.Substring(delimiterPosition + 1);
                }
                else
                {
                    user = uri.UserInfo;
                    password = string.Empty;
                }

                authMechanism = new PlainAuthMechanism(user, password);
            }

            return (uri.Host, port, ssl, vhost, authMechanism);
        }
    }
}