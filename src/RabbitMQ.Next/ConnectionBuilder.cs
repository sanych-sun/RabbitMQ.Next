using System;
using System.Collections.Generic;
using System.Net;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next
{
    public class ConnectionBuilder : IConnectionBuilder
    {
        private const string DefaultLocale = "en-US";

        private readonly IMethodRegistryBuilder methodRegistry = new MethodRegistryBuilder();
        private readonly List<Endpoint> endpoints = new List<Endpoint>();
        private readonly Dictionary<string, object> clientProperties = new Dictionary<string, object>();
        private IAuthMechanism authMechanism;
        private string virtualhost = ProtocolConstants.DefaultVHost;
        private string locale = DefaultLocale;

        private ConnectionBuilder()
        {}

        public static IConnectionBuilder Create()
            =>  new ConnectionBuilder();

        public static IConnectionBuilder Create(string uri)
            => Create(new Uri(WebUtility.UrlDecode(uri)));

        public static IConnectionBuilder Create(Uri uri)
        {
            var parsed = uri.ParseAmqpUri();

            IConnectionBuilder builder = new ConnectionBuilder();
            builder.AddEndpoint(parsed.host, parsed.port, parsed.ssl);
            builder.VirtualHost(parsed.vhost);
            if (parsed.authMechanism != null)
            {
                builder.Auth(parsed.authMechanism);
            }

            return builder;
        }

        IConnectionBuilder IConnectionBuilder.Auth(IAuthMechanism mechanism)
        {
            this.authMechanism = mechanism;
            return this;
        }

        IConnectionBuilder IConnectionBuilder.VirtualHost(string vhost)
        {
            this.virtualhost = vhost;
            return this;
        }

        IConnectionBuilder IConnectionBuilder.AddEndpoint(string host, int port, bool useSsl = false)
        {
            this.endpoints.Add(new Endpoint(host, port, useSsl));
            return this;
        }

        IConnectionBuilder IConnectionBuilder.ConfigureMethodRegistry(Action<IMethodRegistryBuilder> builder)
        {
            builder?.Invoke(this.methodRegistry);
            return this;
        }

        IConnectionBuilder IConnectionBuilder.ClientProperty(string key, object value)
        {
            this.clientProperties[key] = value;
            return this;
        }

        IConnectionBuilder IConnectionBuilder.Locale(string locale)
        {
            this.locale = locale;
            return this;
        }

        public IConnection Build()
            => new Connection(
                this.endpoints.ToArray(),
                this.virtualhost,
                this.authMechanism,
                this.locale,
                this.clientProperties,
                this.methodRegistry.Build());
    }
}