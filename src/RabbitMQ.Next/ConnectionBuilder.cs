using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next;

public class ConnectionBuilder : IConnectionBuilder
{
    private const string DefaultLocale = "en-US";
    private const int DefaultMaxFrameSize = 131_072; // 128kB

    private readonly IConnectionFactory factory;
    private readonly SerializerFactory serializerFactory = new();
    private readonly IMethodRegistryBuilder methodRegistry = new MethodRegistryBuilder();
    private readonly List<Endpoint> endpoints = new();
    private readonly Dictionary<string, object> clientProperties = new();
    private IAuthMechanism authMechanism;
    private string virtualhost = ProtocolConstants.DefaultVHost;
    private string clientLocale = DefaultLocale;
    private int maxFrameSize = DefaultMaxFrameSize;

    private ConnectionBuilder()
        : this(ConnectionFactory.Default)
    {
    }

    internal ConnectionBuilder(IConnectionFactory factory)
    {
        this.factory = factory;
    }

    public static IConnectionBuilder Default
    {
        get
        {
            var builder = new ConnectionBuilder();
            builder.UseDefaults();
            return builder;
        }
    }

    public IConnectionBuilder Auth(IAuthMechanism mechanism)
    {
        this.authMechanism = mechanism;
        return this;
    }

    public IConnectionBuilder VirtualHost(string vhost)
    {
        this.virtualhost = vhost;
        return this;
    }

    public IConnectionBuilder Endpoint(string host, int port, bool ssl)
    {
        this.endpoints.Add(new Endpoint(host, port, ssl));
        return this;
    }

    public IConnectionBuilder ConfigureMethodRegistry(Action<IMethodRegistryBuilder> builder)
    {
        builder?.Invoke(this.methodRegistry);
        return this;
    }

    public IConnectionBuilder ConfigureSerialization(Action<ISerializationBuilder> builder)
    {
        builder?.Invoke(this.serializerFactory);
        return this;
    }

    public  IConnectionBuilder ClientProperty(string key, object value)
    {
        this.clientProperties[key] = value;
        return this;
    }

    public IConnectionBuilder Locale(string locale)
    {
        this.clientLocale = locale;
        return this;
    }

    public IConnectionBuilder MaxFrameSize(int sizeBytes)
    {
        if (sizeBytes < ProtocolConstants.FrameMinSize)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeBytes), "FrameSize cannot be smaller then minimal frame size");
        }

        this.maxFrameSize = sizeBytes;
        return this;
    }

    public Task<IConnection> ConnectAsync(CancellationToken cancellation = default)
    {
        var settings = new ConnectionSettings
        {
            Endpoints = this.endpoints.ToArray(),
            Vhost = this.virtualhost,
            Auth = this.authMechanism,
            Locale = this.clientLocale,
            ClientProperties = this.clientProperties,
            MaxFrameSize = this.maxFrameSize
        };

        return this.factory.ConnectAsync(settings, this.methodRegistry.Build(), this.serializerFactory, cancellation);
    }
}