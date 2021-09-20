using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Sockets;

namespace RabbitMQ.Next.Tests.Mocks
{
    internal class SocketMock : ISocket
    {
        private readonly byte[] buffer;
        private int offset;

        public SocketMock(int bufferSize)
        {
            this.buffer = new byte[bufferSize];
        }

        public void Dispose()
        {
        }

        public ReadOnlyMemory<byte> GetWrittenBytes()
        {
            return ((Memory<byte>) this.buffer)[0..this.offset];
        }

        public ValueTask SendAsync(ReadOnlyMemory<byte> payload)
        {
            this.WriteToBuffer(payload);
            return default;
        }

        public Task FlushAsync() => Task.CompletedTask;

        public int Receive(Memory<byte> buffer) => throw new NotImplementedException();

        private void WriteToBuffer(ReadOnlyMemory<byte> payload)
        {
            var target = ((Memory<byte>) this.buffer)[this.offset..];
            if (payload.Length > target.Length)
            {
                throw new OutOfMemoryException("Out of buffer space, probably have to tune the mock creation.");
            }

            payload.CopyTo(target);
            this.offset += payload.Length;
        }
    }
}