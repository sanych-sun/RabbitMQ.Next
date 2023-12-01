using System;
using System.Collections.Generic;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection;

public readonly struct StartOkMethod : IOutgoingMethod
{
    public StartOkMethod(string mechanism, ReadOnlyMemory<byte> response, string locale, IReadOnlyDictionary<string, object> clientProperties)
    {
        this.Mechanism = mechanism;
        this.Response = response;
        this.Locale = locale;
        this.ClientProperties = clientProperties;
    }

    public MethodId MethodId => MethodId.ConnectionStartOk;

    public IReadOnlyDictionary<string, object> ClientProperties { get; }

    public string Mechanism { get; }

    public ReadOnlyMemory<byte> Response { get; }

    public string Locale { get; }
}