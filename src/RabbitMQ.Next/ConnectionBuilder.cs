using System;
using System.Collections.Generic;
using System.Reflection;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next;

public class ConnectionBuilder : IConnectionBuilder
{
    private const string DefaultLocale = "en-US";
    private const int DefaultMaxFrameSize = 131_072; // 128kB

    private readonly Func<ConnectionSettings, IConnection> factory;
    private readonly List<Endpoint> endpoints = new();
    private readonly Dictionary<string, object> clientProperties = new();
    private IAuthMechanism authMechanism;
    private string virtualhost = ProtocolConstants.DefaultVHost;
    private string clientLocale = DefaultLocale;
    private int maxFrameSize = DefaultMaxFrameSize;
    private ISerializer serializer;

    private ConnectionBuilder()
        : this(s => new Connection(s))
    {
    }

    internal ConnectionBuilder(Func<ConnectionSettings, IConnection> factory)
    {
        this.factory = factory;
    }

    public static IConnectionBuilder Default
    {
        get
        {
            var builder = new ConnectionBuilder();
            builder
                .ClientProperty("product", "RabbitMQ.Next")
                .ClientProperty("version", Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "undefined")
                .ClientProperty("platform", Environment.OSVersion.ToString())
                .ClientProperty("capabilities", new Dictionary<string, object>
                {
                    ["authentication_failure_close"] = true,
                });
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
    
    public IConnectionBuilder UseSerializer(ISerializer serializer)
    {
        this.serializer = serializer;
        return this;
    }

    public IConnection Build()
    {
        var settings = new ConnectionSettings
        {
            Endpoints = this.endpoints.ToArray(),
            Vhost = this.virtualhost,
            Auth = this.authMechanism,
            Locale = this.clientLocale,
            ClientProperties = this.clientProperties,
            MaxFrameSize = this.maxFrameSize,
            Serializer = this.serializer,
        };

        return this.factory(settings);
    }
}
