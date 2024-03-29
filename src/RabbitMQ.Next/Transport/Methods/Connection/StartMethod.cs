using System.Collections.Generic;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection;

public readonly struct StartMethod : IIncomingMethod
{
    public StartMethod(byte versionMajor, byte versionMinor, string mechanisms, string locales, IReadOnlyDictionary<string, object> properties)
    {
        this.VersionMajor = versionMajor;
        this.VersionMinor = versionMinor;
        this.Mechanisms = mechanisms;
        this.Locales = locales;
        this.ServerProperties = properties;
    }

    public MethodId MethodId => MethodId.ConnectionStart;

    public byte VersionMajor { get; }

    public byte VersionMinor { get; }

    public IReadOnlyDictionary<string, object> ServerProperties { get; }

    public string Mechanisms { get; }

    public string Locales { get; }
}