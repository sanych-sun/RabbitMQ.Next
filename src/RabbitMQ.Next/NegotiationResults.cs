using System;

namespace RabbitMQ.Next;

internal class NegotiationResults
{
    public NegotiationResults(int channelMax, int frameMaxSize, TimeSpan heartbeatInterval)
    {
        this.ChannelMax = channelMax;
        this.FrameMaxSize = frameMaxSize;
        this.HeartbeatInterval = heartbeatInterval;
    }

    public int ChannelMax { get; }

    public int FrameMaxSize { get; }

    public TimeSpan HeartbeatInterval { get; }
}
