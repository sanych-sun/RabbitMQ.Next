using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;

namespace RabbitMQ.Next.Transport.Channels
{
    internal sealed class ChannelWriter : IBufferWriter<byte>, IDisposable
    {
        private PipeWriter writer;
        private long dataSize;

        public ChannelWriter(PipeWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            this.writer = writer;
        }

        public void Dispose()
        {
            this.writer?.Complete();
            this.writer = null;
        }

        public void BeginLogicalFrame(ChannelFrameType type, int size)
        {
            this.EnsureWriter();
            if (this.dataSize != 0)
            {
                // todo: connection exceptions?
                throw new InvalidOperationException();
            }

            this.writer.WriteChannelHeader(type, size);
            this.dataSize = size;
        }


        public void Advance(int count)
        {
            this.EnsureWriter();
            this.writer.Advance(count);
            this.dataSize -= count;

            if (this.dataSize < 0)
            {
                // todo: connection exceptions?
                throw new InvalidOperationException();
            }
        }

        public async ValueTask FlushAsync()
        {
            this.EnsureWriter();
            if (this.dataSize != 0)
            {
                // todo: connection exceptions?
                throw new InvalidOperationException();
            }

            await this.writer.FlushAsync();
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            this.EnsureWriter();
            return this.writer.GetMemory(sizeHint);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            this.EnsureWriter();
            return this.writer.GetSpan(sizeHint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureWriter()
        {
            if (this.writer == null)
            {
                throw new InvalidOperationException("Cannot use completed ChannelWriter.");
            }
        }
    }
}