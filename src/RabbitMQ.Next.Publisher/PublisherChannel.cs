using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;

namespace RabbitMQ.Next.Publisher;

internal sealed class PublisherChannel
{
    private readonly SemaphoreSlim channelInitializationSync = new(1,1);
    private readonly IConnection connection;
    private readonly List<Func<IChannel, ValueTask>> initializers = new();
    private IChannel channel;

    public PublisherChannel(IConnection connection)
    {
        this.connection = connection;
    }

    public void OnChannelCreated(Func<IChannel, ValueTask> initializer)
    {
        ArgumentNullException.ThrowIfNull(initializer);
        this.initializers.Add(initializer);
    }

    public async ValueTask<IChannel> GetChannelAsync(CancellationToken cancellation)
    {
        if (this.channel != null)
        {
            return this.channel;
        }

        await this.channelInitializationSync.WaitAsync(cancellation).ConfigureAwait(false);
        try
        {
            if (this.channel == null)
            {   
                var ch = await this.connection.OpenChannelAsync(cancellation);

                for (var i = 0; i < this.initializers.Count; i++)
                {
                    await this.initializers[i].Invoke(ch);
                }

                this.channel = ch;
            }
        }
        finally
        {
            this.channelInitializationSync.Release();
        }

        return this.channel;
        
        
        
        
    }
}