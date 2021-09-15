using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Messaging;

namespace RabbitMQ.Next.Channels
{
    internal class FrameBuilder : IFrameBuilder, IBufferWriter<byte>
    {
        private readonly IBufferPool pool;
        private readonly ushort channelNumber;
        private readonly int frameMaxSize;
        private readonly List<IMemoryOwner<byte>> chunks;
        private MemoryBlock buffer;
        private int offset;
        private int currentFrameSize;
        private long totalContentSize;
        private Memory<byte> contentSizeBlock;
        private Memory<byte> currentFrameHeader;
        private FrameType currentFrameType;

        public FrameBuilder(IBufferPool pool, ushort channelNumber, int frameMaxSize)
        {
            this.chunks = new List<IMemoryOwner<byte>>();
            this.pool = pool;
            this.channelNumber = channelNumber;
            this.frameMaxSize = frameMaxSize;
        }

        public IBufferWriter<byte> BeginMethodFrame(MethodId methodId)
        {
            if (this.currentFrameType != FrameType.Malformed)
            {
                throw new InvalidOperationException("Cannot start new frame before completing previous one.");
            }

            this.currentFrameType = FrameType.Method;
            this.EnsureBuffer();

            // allocate space for generic frame header
            this.currentFrameHeader = this.buffer.Memory.Slice(this.offset, ProtocolConstants.FrameHeaderSize);
            this.offset += ProtocolConstants.FrameHeaderSize;
            
            // write methodId
            this.buffer.Memory[this.offset..].Write((uint) methodId);
            this.offset += sizeof(uint);
            this.currentFrameSize += sizeof(uint);
            
            // rest of the frame should contain methodArgs
            return this;
        }

        public IBufferWriter<byte> BeginContentFrame(IMessageProperties properties)
        {
            if (this.currentFrameType != FrameType.Malformed)
            {
                throw new InvalidOperationException("Cannot start new frame before completing previous one.");
            }

            this.currentFrameType = FrameType.ContentHeader;
            this.EnsureBuffer();

            this.currentFrameHeader = this.buffer.Memory.Slice(this.offset, ProtocolConstants.FrameHeaderSize);
            this.offset += ProtocolConstants.FrameHeaderSize;
            this.contentSizeBlock = this.buffer.Memory.Slice(this.offset + 4, 8);
            this.currentFrameSize = this.buffer.Memory[this.offset..].WriteContentHeader(properties, 0);
            this.offset += this.currentFrameSize;
            this.EndFrame();

            this.currentFrameType = FrameType.ContentBody;
            this.currentFrameHeader = this.buffer.Memory.Slice(this.offset, ProtocolConstants.FrameHeaderSize);
            this.offset += ProtocolConstants.FrameHeaderSize;

            this.totalContentSize = 0;
            return this;
        }


        public void EndFrame()
        {
            if (this.currentFrameType == FrameType.Malformed)
            {
                throw new InvalidOperationException("BeginFrame should be called before EndFrame.");
            }

            if (this.currentFrameType == FrameType.ContentBody)
            {
                this.totalContentSize += this.currentFrameSize;
                this.contentSizeBlock.Write((ulong)this.totalContentSize);
            }

            this.currentFrameHeader.WriteFrameHeader(this.currentFrameType, this.channelNumber, (uint)this.currentFrameSize);
            this.buffer.Memory[this.offset..].Write(ProtocolConstants.FrameEndByte);
            this.offset++;
            this.currentFrameType = FrameType.Malformed;
            this.currentFrameSize = 0;
        }

        public ValueTask WriteToAsync(ChannelWriter<IMemoryOwner<byte>> channel)
        {
            if (this.chunks.Count == 0)
            {
                this.buffer.Slice(this.offset);
                var single = this.buffer;
                this.buffer = default;
                return WriteChunkAsync(channel, single);
            }

            this.FinalizeCurrentBuffer();
            return WriteMultipleChunksAsync(channel, this.chunks);
        }

        public void Reset()
        {
            this.chunks.Clear();
            this.offset = 0;
            this.currentFrameType = FrameType.Malformed;
            this.currentFrameSize = 0;
            this.totalContentSize = 0;
            this.contentSizeBlock = default;
            this.currentFrameHeader = default;
        }

        void IBufferWriter<byte>.Advance(int count)
        {
            this.currentFrameSize += count;
            this.offset += count;

            if (this.currentFrameSize > this.frameMaxSize)
            {
                throw new OutOfMemoryException();
            }
        }

        Memory<byte> IBufferWriter<byte>.GetMemory(int sizeHint)
        {
            var size = this.ExpandIfRequired(sizeHint);
            return this.buffer.Memory.Slice(this.offset, size);
        }

        Span<byte> IBufferWriter<byte>.GetSpan(int sizeHint)
        {
            var size = this.ExpandIfRequired(sizeHint);
            return this.buffer.Memory.Span.Slice(this.offset, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ExpandIfRequired(int requestedSize)
        {
            var bufferAvailable = this.buffer.BufferCapacity - this.offset - 1;
            if (this.buffer.BufferCapacity - this.offset - 1 > requestedSize) // current buffer available space: capacity - already written bytes - frame end
            {
                return Math.Min(bufferAvailable, this.frameMaxSize - this.currentFrameSize);
            }

            this.EndFrame();
            this.FinalizeCurrentBuffer();

            this.currentFrameType = FrameType.ContentBody;
            this.buffer = this.pool.CreateMemory();
            this.currentFrameHeader = this.buffer.Memory.Slice(this.offset, ProtocolConstants.FrameHeaderSize);
            this.offset += ProtocolConstants.FrameHeaderSize;
            return this.frameMaxSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureBuffer()
        {
            this.buffer ??= this.pool.CreateMemory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FinalizeCurrentBuffer()
        {
            this.buffer.Slice(this.offset);
            this.chunks.Add(this.buffer);
            this.offset = 0;
            this.buffer = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ValueTask WriteChunkAsync(ChannelWriter<IMemoryOwner<byte>> channel, IMemoryOwner<byte> chunk)
        {
            if (channel.TryWrite(chunk))
            {
                return default;
            }

            return channel.WriteAsync(chunk);
        }

        private static async ValueTask WriteMultipleChunksAsync(ChannelWriter<IMemoryOwner<byte>> channel, IReadOnlyList<IMemoryOwner<byte>> chunks)
        {
            for(var i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];

                if (!channel.TryWrite(chunk))
                {
                    await channel.WriteAsync(chunk);
                }
            }
        }
    }
}