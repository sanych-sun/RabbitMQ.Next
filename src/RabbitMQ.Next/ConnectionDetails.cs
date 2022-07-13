namespace RabbitMQ.Next;

internal class ConnectionDetails
{
    public ConnectionDetails(ConnectionSettings settings)
    {
        this.Settings = settings;
    }

    public ConnectionSettings Settings { get; }

    public NegotiationResults Negotiated { get; set; }

    public string RemoteHost { get; set; }

    public string RemotePort { get; set; }

    public bool IsSsl { get; set; }

    public string VirtualHost { get; set; }
}