using System;
using System.Buffers;
using System.IO.Pipelines;
using RabbitMQ.Next.Abstractions.Channels;

namespace RabbitMQ.Next.Transport.Channels
{
    internal sealed class PipeWrapper : IBufferWriter<byte>
    {
        private readonly PipeWriter writer;
        private long dataSize;

        public PipeWrapper(PipeWriter writer)
        {
            this.writer = writer;
        }

        public void BeginLogicalFrame(ChannelFrameType type, int size)
        {
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
            this.writer.Advance(count);
            this.dataSize -= count;

            if (this.dataSize == 0)
            {
                this.writer.FlushAsync().GetAwaiter().GetResult();
            }

            if (this.dataSize < 0)
            {
                // todo: connection exceptions?
                throw new InvalidOperationException();
            }
        }

        public Memory<byte> GetMemory(int sizeHint = 0) => this.writer.GetMemory(sizeHint);

        public Span<byte> GetSpan(int sizeHint = 0) => this.writer.GetSpan(sizeHint);
    }
}