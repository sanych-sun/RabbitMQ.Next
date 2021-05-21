using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace RabbitMQ.Next.Transport.Channels
{
    internal class ChannelPool
    {
        private readonly ReaderWriterLockSlim channelsLock;
        private readonly ConcurrentQueue<ushort> releasedItems;
        private IChannelInternal[] channels;
        private int lastId = -1;

        public ChannelPool(int initialPoolSize = 10)
        {
            this.channelsLock = new ReaderWriterLockSlim();
            this.releasedItems = new ConcurrentQueue<ushort>();
            this.channels = new IChannelInternal[initialPoolSize];
        }

        public ushort Register(IChannelInternal channel)
        {
            var nextIndex = this.GetNextIndex();
            this.AssignChannel(nextIndex, channel);
            return nextIndex;
        }

        public void Release(ushort channelNumber, Exception ex = null)
        {
            var channel = this.AssignChannel(channelNumber, null);
            channel?.SetCompleted(ex);
        }

        public void ReleaseAll(Exception ex = null)
        {
            this.channelsLock.EnterWriteLock();

            try
            {
                for (var i = 0; i < this.channels.Length; i++)
                {
                    this.channels[i]?.SetCompleted(ex);
                    this.channels[i] = null;
                }

                this.releasedItems.Clear();
                this.lastId = -1;
            }
            finally
            {
                this.channelsLock.ExitWriteLock();
            }
        }

        public IChannelInternal this[int i]
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

        private ushort GetNextIndex()
        {
            if (this.releasedItems.TryDequeue(out var result))
            {
                return result;
            }

            var nextId = (ushort)Interlocked.Increment(ref this.lastId);
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
                    this.channels = new IChannelInternal[channelsTmp.Length * 2];
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
        private IChannelInternal AssignChannel(int index, IChannelInternal channel)
        {
            IChannelInternal prev = null;
            this.channelsLock.EnterWriteLock();

            try
            {
                prev = this.channels[index];
                this.channels[index] = channel;
            }
            finally
            {
                this.channelsLock.ExitWriteLock();
            }

            return prev;
        }

    }
}