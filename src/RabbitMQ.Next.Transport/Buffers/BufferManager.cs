using System.Collections.Concurrent;

namespace RabbitMQ.Next.Transport.Buffers
{
    internal class BufferManager
    {
        private readonly ConcurrentBag<byte[]> releasedItems;
        private int bufferSize;

        public BufferManager(int bufferSize)
        {
            this.releasedItems = new ConcurrentBag<byte[]>();
            this.bufferSize = bufferSize;
        }

        public byte[] Rent()
        {
            if (this.releasedItems.TryTake(out var result))
            {
                return result;
            }

            return new byte[this.bufferSize];
        }

        public void Return(byte[] buffer)
        {
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
    }
}