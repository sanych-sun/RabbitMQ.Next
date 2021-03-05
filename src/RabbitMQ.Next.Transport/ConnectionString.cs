using System;
using System.Collections.Generic;
using System.Net;

namespace RabbitMQ.Next.Transport
{
    public class ConnectionString
    {
        // TODO: add amqps support
        private const string DefaultSchemaName = "amqp";
        private const string SslSchemaName = "amqps";
        private const int DefaultPort = 5672;
        private const int SslDefaultPort = 5671;
        private const string DefaultUserName = "guest";
        private const string DefaultPassword = "guest";
        private const string DefaultVHost = "/";

        public ConnectionString(bool ssl, IReadOnlyList<Endpoint> endpoints, string userName, string password, string virtualHost = DefaultVHost)
        {
            this.Ssl = ssl;
            this.EndPoints = endpoints;
            this.UserName = userName;
            this.Password = password;
            this.VirtualHost = virtualHost;
        }

        public static ConnectionString Create(string uri) => Create(new Uri(WebUtility.UrlDecode(uri)));

        public static ConnectionString Create(Uri uri)
        {
            if (uri.Scheme != DefaultSchemaName && uri.Scheme != SslSchemaName)
            {
                throw new NotSupportedException();
            }

            var ssl = uri.Scheme == SslSchemaName;

            var endpoints = new[]
            {
                new Endpoint(uri.Host, uri.IsDefaultPort ? ssl ? SslDefaultPort : DefaultPort : uri.Port)
            };
            var vHost = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
            if (string.IsNullOrEmpty(vHost))
            {
                vHost = DefaultVHost;
            }
            var userName = DefaultUserName;
            var password = DefaultPassword;
            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                var delimeterPosition = uri.UserInfo.IndexOf(':');
                if (delimeterPosition >= 0)
                {
                    userName = uri.UserInfo.Substring(0, delimeterPosition);
                    password = uri.UserInfo.Substring(delimeterPosition + 1);
                }
                else
                {
                    userName = uri.UserInfo;
                    password = string.Empty;
                }
            }

            return new ConnectionString(ssl, endpoints, userName, password, vHost);
        }

        public IReadOnlyList<Endpoint> EndPoints { get; }

        public string UserName { get; }

        public string Password { get; }

        public string VirtualHost { get; }

        public bool Ssl { get; }
    }
}