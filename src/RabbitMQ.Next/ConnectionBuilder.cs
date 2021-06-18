using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next
{
    public class ConnectionBuilder : IConnectionBuilder
    {
        private const string DefaultLocale = "en-US";

        private readonly IConnectionFactory factory;
        private readonly IMethodRegistryBuilder methodRegistry = new MethodRegistryBuilder();
        private readonly List<Endpoint> endpoints = new List<Endpoint>();
        private readonly Dictionary<string, object> clientProperties = new Dictionary<string, object>();
        private IAuthMechanism authMechanism;
        private string virtualhost = ProtocolConstants.DefaultVHost;
        private string locale = DefaultLocale;

        public ConnectionBuilder(bool useDefaults = true)
            : this(ConnectionFactory.Default)
        {
            if (useDefaults)
            {
                this.UseDefaults();
            }
        }

        internal ConnectionBuilder(IConnectionFactory factory)
        {
            this.factory = factory;

            this.clientProperties["capabilities"] = new Dictionary<string, object>
            {
                ["authentication_failure_close"] = true,
            };
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

        IConnectionBuilder IConnectionBuilder.AddEndpoint(string host, int port, bool ssl)
        {
            this.endpoints.Add(new Endpoint(host, port, ssl));
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

        public Task<IConnection> ConnectAsync()
            => this.factory.ConnectAsync(
                this.endpoints.ToArray(),
                this.virtualhost,
                this.authMechanism,
                this.locale,
                this.clientProperties,
                this.methodRegistry.Build());
    }
}