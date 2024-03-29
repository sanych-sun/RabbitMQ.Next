using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace RabbitMQ.Next.Channels;

internal class ChannelPool
{
    private readonly object syncRoot = new();
    private readonly Func<ushort, IChannelInternal> factory;
    private readonly ConcurrentQueue<ushort> releasedItems;
    private IChannelInternal[] channels;
    private int lastId;

    public ChannelPool(Func<ushort, IChannelInternal> factory, int initialPoolSize = 10)
    {
        this.factory = factory;
        this.releasedItems = new ConcurrentQueue<ushort>();
        this.channels = new IChannelInternal[initialPoolSize]; 
    }

    public IChannelInternal Create()
    {
        var channelNumber = this.GetNextIndex();
        var channel = this.factory(channelNumber);
        channel.Completion.ContinueWith(_ =>
        {
            this.AssignChannel(channelNumber, null);
            this.releasedItems.Enqueue(channelNumber);
        });

        this.AssignChannel(channelNumber, channel);
        return channel;
    }

    public IChannelInternal Get(ushort channelNumber)
    {
        if (channelNumber >= this.channels.Length)
        {
            throw new ArgumentOutOfRangeException();
        }

        var result = this.channels[channelNumber];

        if (result == null)
        {
            throw new ArgumentOutOfRangeException();
        }

        return result;
    }

    public void ReleaseAll(Exception ex = null)
    {
        for (var i = this.lastId; i >= 0; i--)
        {
            this.channels[i]?.TryComplete(ex);
        }
    }

    private ushort GetNextIndex()
    {
        if (this.releasedItems.TryDequeue(out var result))
        {
            return result;
        }

        return (ushort)Interlocked.Increment(ref this.lastId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AssignChannel(int index, IChannelInternal channel)
    {
        lock(this.syncRoot)
        {
            if (index >= this.channels.Length)
            {
                var channelsTmp = this.channels;
                this.channels = new IChannelInternal[channelsTmp.Length * 2];
                channelsTmp.CopyTo(this.channels, 0);
            }

            this.channels[index] = channel;
        }
    }
}