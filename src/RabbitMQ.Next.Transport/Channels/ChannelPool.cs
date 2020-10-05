using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace RabbitMQ.Next.Transport.Channels
{
    internal class ChannelPool
    {
        private readonly Func<int, Channel> channelFactory;
        private readonly ReaderWriterLockSlim channelsLock;
        private readonly ConcurrentQueue<int> releasedItems;
        private Channel[] channels;
        private int lastId = -1;

        public ChannelPool(Func<int, Channel> channelFactory, int initialPoolSize = 10)
        {
            this.channelFactory = channelFactory;
            this.channelsLock = new ReaderWriterLockSlim();
            this.releasedItems = new ConcurrentQueue<int>();
            this.channels = new Channel[initialPoolSize];
        }

        public Channel Next()
        {
            var nextIndex = this.GetNextIndex();
            return this.Create(nextIndex);
        }

        public void Release(int index)
        {
            var channel = this[index];

            this.AssignChannel(index, null);
            this.releasedItems.Enqueue(index);

            channel.Dispose();
        }

        public void ReleaseAll()
        {
            this.channelsLock.EnterWriteLock();

            try
            {
                for (var i = 0; i < this.channels.Length; i++)
                {
                    if (this.channels[i] == null)
                    {
                        continue;
                    }

                    this.channels[i].Dispose();
                    this.channels[i] = null;
                }

                this.lastId = -1;
            }
            finally
            {
                this.channelsLock.ExitWriteLock();
            }
        }

        public Channel this[int i]
        {
            get
            {
                if (i > this.lastId)
                {
                    throw new ArgumentOutOfRangeException();
                }

                var result = this.channels[i];

                if (result == null)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return result;
            }
        }

        private Channel Create(int index)
        {
            var channel = this.channelFactory(index);
            this.AssignChannel(index, channel);
            return channel;
        }

        private int GetNextIndex()
        {
            if (this.releasedItems.TryDequeue(out var result))
            {
                return result;
            }

            var nextId = Interlocked.Increment(ref this.lastId);
            this.channelsLock.EnterReadLock();
            try
            {
                if (nextId < this.channels.Length)
                {
                    return nextId;
                }
            }
            finally
            {
                this.channelsLock.ExitReadLock();
            }

            this.channelsLock.EnterWriteLock();
            try
            {
                if (nextId < this.channels.Length)
                {
                    var channelsTmp = this.channels;
                    this.channels = new Channel[channelsTmp.Length * 2];
                    channelsTmp.CopyTo(this.channels, 0);
                }
            }
            finally
            {
                this.channelsLock.ExitWriteLock();
            }

            return nextId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AssignChannel(int index, Channel channel)
        {
            this.channelsLock.EnterWriteLock();

            try
            {
                this.channels[index] = channel;
            }
            finally
            {
                this.channelsLock.ExitWriteLock();
            }
        }

    }
}