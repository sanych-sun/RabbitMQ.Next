using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Transport.Messaging
{
    // TODO: make the object reusable and pool them to reduce allocation
    internal class FrameBuilder : IFrameBuilder, IBufferWriter<byte>
    {
        private readonly IBufferPool pool;
        private readonly ushort channelNumber;
        private readonly int frameMaxSize;
        private readonly int singleFrameSize;
        private List<MemoryBlock> chunks;
        private MemoryBlock buffer;
        private int offset;
        private int currentFrameSize;
        private long totalContentSize;
        private Memory<byte> contentSizeBlock;
        private Memory<byte> currentFrameHeader;
        private FrameType currentFrameType;

        public FrameBuilder(IBufferPool pool, ushort channelNumber, int frameMaxSize)
        {
            this.pool = pool;
            this.channelNumber = channelNumber;
            this.frameMaxSize = frameMaxSize;
            this.singleFrameSize = ProtocolConstants.FrameHeaderSize + frameMaxSize + 1;
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
            this.buffer.Memory.Slice(this.offset).Span.Write((uint) methodId);
            this.offset += sizeof(uint);
            this.currentFrameSize += sizeof(uint);
            
            // rest of the frame should contain methodArgs
            return this;
        }

        public IBufferWriter<byte> BeginContentFrame(MessageProperties properties)
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
            this.currentFrameSize = this.buffer.Memory.Span.Slice(this.offset).WriteContentHeader(properties, 0);
            this.offset += this.currentFrameSize;
            this.EndFrame();

            this.currentFrameType = FrameType.ContentBody;
            this.EnsureBuffer();
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
                this.contentSizeBlock.Span.Write((ulong)this.totalContentSize);
            }

            this.currentFrameHeader.Span.WriteFrameHeader(this.currentFrameType, this.channelNumber, (uint)this.currentFrameSize);
            this.buffer.Memory.Slice(this.offset).Span.Write(ProtocolConstants.FrameEndByte);
            this.offset++;
            this.currentFrameType = FrameType.Malformed;
            this.currentFrameSize = 0;
        }

        public ValueTask WriteToAsync(ChannelWriter<MemoryBlock> channel)
        {
            if (this.chunks == null)
            {
                this.buffer.Slice(this.offset);
                var single = this.buffer;
                this.buffer = default;
                return WriteChunkAsync(channel, single);
            }

            this.FinalizeCurrentBuffer();
            var copy = this.chunks;
            this.chunks = null;

            return WriteMultipleChunksAsync(channel, copy);
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
            this.ExpandIfRequired(sizeHint);
            return this.buffer.Memory.Slice(this.offset, sizeHint);
        }

        Span<byte> IBufferWriter<byte>.GetSpan(int sizeHint)
        {
            this.ExpandIfRequired(sizeHint);
            return this.buffer.Memory.Span.Slice(this.offset, sizeHint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ExpandIfRequired(int requestedSize)
        {
            if (this.currentFrameSize + requestedSize <= this.frameMaxSize)
            {
                return;
            }

            this.EndFrame();

            this.currentFrameType = FrameType.ContentBody;
            this.EnsureBuffer();
            this.currentFrameHeader = this.buffer.Memory.Slice(this.offset, ProtocolConstants.FrameHeaderSize);
            this.offset += ProtocolConstants.FrameHeaderSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureBuffer()
        {
            if (this.buffer.Memory.IsEmpty)
            {
                this.buffer = this.pool.CreateMemory();
            }
            else if (this.buffer.Memory.Length - this.offset < this.singleFrameSize)
            {
                this.FinalizeCurrentBuffer();
                this.buffer = this.pool.CreateMemory();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FinalizeCurrentBuffer()
        {
            this.chunks ??= new List<MemoryBlock>();
            this.buffer.Slice(this.offset);
            this.chunks.Add(this.buffer);
            this.offset = 0;
            this.buffer = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ValueTask WriteChunkAsync(ChannelWriter<MemoryBlock> channel, MemoryBlock chunk)
        {
            if (channel.TryWrite(chunk))
            {
                return default;
            }

            return channel.WriteAsync(chunk);
        }

        private static async ValueTask WriteMultipleChunksAsync(ChannelWriter<MemoryBlock> channel, IReadOnlyList<MemoryBlock> chunks)
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