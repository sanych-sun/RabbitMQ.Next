using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Abstractions.Buffers;

namespace RabbitMQ.Next.Buffers
{
    internal class BufferManager : IBufferManager
    {
        private readonly ObjectPool<byte[]> arrayPool;
        private readonly ConcurrentBag<byte[]> releasedItems;
        private int bufferSize;

        public BufferManager(int bufferSize)
        {
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            this.releasedItems = new ConcurrentBag<byte[]>();
            this.bufferSize = bufferSize;
        }

        public byte[] Rent(int minSize = 0)
        {
            if (minSize > this.bufferSize)
            {
                return new byte[minSize];
            }

            if (this.releasedItems.TryTake(out var result))
            {
                return result;
            }

            return new byte[this.bufferSize];
        }

        public void Release(byte[] buffer)
        {
            if (buffer == null)
            {
                return;
            }

            if (buffer.Length != this.bufferSize)
            {
                return;
            }

            this.releasedItems.Add(buffer);
        }

        public void SetBufferSize(int size)
        {
            if (this.bufferSize == size)
            {
                return;
            }

            this.bufferSize = size;
            this.releasedItems.Clear();
        }

        public int BufferSize => this.bufferSize;

        internal int ReleasedItemsCount() => this.releasedItems.Count;
    }
}