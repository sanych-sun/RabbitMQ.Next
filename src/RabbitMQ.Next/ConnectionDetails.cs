using System;

namespace RabbitMQ.Next;

internal class ConnectionDetails
{
    public ConnectionDetails(ConnectionSettings settings)
    {
        this.Settings = settings;
    }

    public ConnectionSettings Settings { get; }
    
    public int? ChannelMax { get; private set; }

    public int? FrameMaxSize { get; private set; }

    public TimeSpan? HeartbeatInterval { get; private set; }
    
    public void PopulateWithNegotiationResults(NegotiationResults negotiationResults)
    {
        ArgumentNullException.ThrowIfNull(negotiationResults);
        
        this.ChannelMax = negotiationResults.ChannelMax;
        this.FrameMaxSize = negotiationResults.FrameMaxSize;
        this.HeartbeatInterval = negotiationResults.HeartbeatInterval;
    }
}
