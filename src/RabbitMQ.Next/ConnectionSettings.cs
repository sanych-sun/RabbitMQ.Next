using System.Collections.Generic;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next;

internal class ConnectionSettings
{
    public IReadOnlyList<Endpoint> Endpoints { get; init; }

    public string Vhost { get; init; }

    public string Locale { get; init; }

    public IAuthMechanism Auth { get; init; }

    public int MaxFrameSize { get; init; }

    public IReadOnlyDictionary<string, object> ClientProperties { get; init; }
}