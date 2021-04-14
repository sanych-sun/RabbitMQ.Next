using System;
using System.Buffers;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    public struct Content : IDisposable
    {
        private readonly ISerializer serializer;
        private ReadOnlySequence<byte> payload;
        private bool isDisposed;

        public Content(ISerializer serializer, ReadOnlySequence<byte> payload)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            this.serializer = serializer;
            this.payload = payload;
            this.isDisposed = false;
        }

        public T GetContent<T>()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(Content));
            }

            return this.serializer.Deserialize<T>(this.payload);
        }

        public void Dispose()
        {
            this.payload = ReadOnlySequence<byte>.Empty;
            this.isDisposed = true;
        }
    }
}