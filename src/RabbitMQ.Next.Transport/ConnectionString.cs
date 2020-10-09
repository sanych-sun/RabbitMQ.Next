using System;
using System.Collections.Generic;
using System.Net;

namespace RabbitMQ.Next.Transport
{
    public class ConnectionString
    {
        // TODO: add amqps support
        private const string SchemaName = "amqp";
        private const int DefaultPort = 5672;
        private const string DefaultUserName = "guest";
        private const string DefaultPassword = "guest";
        private const string DefaultVHost = "/";

        public ConnectionString(IReadOnlyList<Endpoint> endpoints, string userName, string password, string virtualHost = DefaultVHost)
        {
            this.EndPoints = endpoints;
            this.UserName = userName;
            this.Password = password;
            this.VirtualHost = virtualHost;
        }

        public static ConnectionString Create(string uri) => Create(new Uri(WebUtility.UrlDecode(uri)));

        public static ConnectionString Create(Uri uri)
        {
            if (uri.Scheme != SchemaName)
            {
                throw new NotSupportedException();
            }

            var endpoints = new[]
            {
                new Endpoint(uri.Host, uri.IsDefaultPort ? DefaultPort : uri.Port)
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

            return new ConnectionString(endpoints, userName, password, vHost);
        }

        public IReadOnlyList<Endpoint> EndPoints { get; }

        public string UserName { get; }

        public string Password { get; }

        public string VirtualHost { get; }
    }
}