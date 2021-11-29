using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Messaging;

namespace RabbitMQ.Next.Channels
{
    internal class FrameBuilder
    {
        private readonly ObjectPool<MemoryBlock> memoryPool;
        private readonly List<MemoryBlock> chunks;
        private readonly ContentBufferWriter contentBufferWriter;
        private ushort chNumber;
        private int frameMaxSize;
        private MemoryBlock buffer;
        private Memory<byte> currentFrameHeader;

        public FrameBuilder(ObjectPool<MemoryBlock> memoryPool)
        {
            this.chunks = new List<MemoryBlock>();
            this.memoryPool = memoryPool;
            this.contentBufferWriter = new ContentBufferWriter(this);
            this.chNumber = ushort.MaxValue;
        }

        public void Initialize(ushort channelNumber, int frameMaxSize)
        {
            this.chNumber = channelNumber;
            this.frameMaxSize = frameMaxSize;
        }

        public void WriteMethodFrame<TMethod>(TMethod method, IMethodFormatter<TMethod> formatter)
            where TMethod: struct, IOutgoingMethod
        {
            var payloadBuffer = this.BeginFrame()
                .Write((uint)method.MethodId);

            var payloadSize = formatter.Write(payloadBuffer, method) + sizeof(uint);
            this.buffer.Commit(payloadSize);
            this.EndFrame(FrameType.Method, (uint)payloadSize);
        }

        public void WriteContentFrame<TState>(TState state, IMessageProperties properties, Action<TState, IBufferWriter<byte>> contentBuilder)
        {
            var payloadBuffer = this.BeginFrame();

            var result = payloadBuffer
                .Write((ushort) ClassId.Basic)
                .Write((ushort) ProtocolConstants.ObsoleteField)
                .Slice(0, sizeof(ulong), out var contentSizeBuffer)
                .WriteMessageProperties(properties);

            var payloadSize = payloadBuffer.Length - result.Length;
            this.buffer.Commit(payloadSize);
            this.EndFrame(FrameType.ContentHeader, (uint)payloadSize);

            this.BeginFrame();
            contentBuilder.Invoke(state, this.contentBufferWriter);
            this.EndFrame(FrameType.ContentBody, (uint)this.contentBufferWriter.CurrentFrameBytes);

            contentSizeBuffer.Write((ulong)this.contentBufferWriter.TotalContentBytes);
        }


        // allocates space for generic frame header and returns memory available for payload
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Memory<byte> BeginFrame()
        {
            if (this.chNumber == ushort.MaxValue)
            {
                throw new InvalidOperationException("Cannot use non-initialized FrameBuilder.");
            }

            this.EnsureBuffer();

            this.currentFrameHeader = this.buffer.Writer[..ProtocolConstants.FrameHeaderSize];
            this.buffer.Commit(ProtocolConstants.FrameHeaderSize);

            return this.buffer.Writer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EndFrame(FrameType type, uint payloadSize)
        {
            this.currentFrameHeader.WriteFrameHeader(type, this.chNumber, payloadSize);
            this.buffer.Writer.Span[0] = ProtocolConstants.FrameEndByte;
            this.buffer.Commit(1);
        }

        public ValueTask WriteToAsync(ChannelWriter<MemoryBlock> channel)
        {
            if (this.chunks.Count > 0)
            {
                return this.WriteMultipartAsync(channel);
            }

            if (channel.TryWrite(this.buffer))
            {
                return default;
            }

            return channel.WriteAsync(this.buffer);
        }

        private async ValueTask WriteMultipartAsync(ChannelWriter<MemoryBlock> channel)
        {
            this.RotateBuffers();
            for(var i = 0; i < this.chunks.Count; i++)
            {
                var chunk = this.chunks[i];

                if (!channel.TryWrite(chunk))
                {
                    await channel.WriteAsync(chunk);
                }
            }
        }

        public void Reset()
        {
            // Should not release memory blocks here! It will be done on the frame sending in Connection.SendLoop
            this.buffer = default;
            this.chunks.Clear();
            this.currentFrameHeader = default;
            this.contentBufferWriter.Reset();
            this.chNumber = ushort.MaxValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureBuffer()
        {
            this.buffer ??= this.memoryPool.Get();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RotateBuffers()
        {
            if (this.buffer != default)
            {
                this.chunks.Add(this.buffer);
            }

            this.buffer = default;
        }

        private class ContentBufferWriter : IBufferWriter<byte>
        {
            private readonly FrameBuilder owner;

            public ContentBufferWriter(FrameBuilder owner)
            {
                this.owner = owner;
            }

            public long TotalContentBytes { get; private set; }

            public int CurrentFrameBytes { get; private set; }

            public void Reset()
            {
                this.TotalContentBytes = 0;
                this.CurrentFrameBytes = 0;
            }

            void IBufferWriter<byte>.Advance(int count)
            {
                this.CurrentFrameBytes += count;
                this.TotalContentBytes += count;

                this.owner.buffer.Commit(count);
            }

            public Memory<byte> GetMemory(int sizeHint)
            {
                var size = this.ExpandIfRequired(sizeHint);
                return this.owner.buffer.Writer[..size];
            }

            public Span<byte> GetSpan(int sizeHint)
                => this.GetMemory(sizeHint).Span;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private int ExpandIfRequired(int requestedSize)
            {
                // todo: implement workaround for too big chunks by allocating some extra array with merging the data back into buffer on Advance
                if (requestedSize > this.owner.frameMaxSize)
                {
                    throw new OutOfMemoryException();
                }

                // current buffer available space: capacity - frame end
                var bufferAvailable = this.owner.buffer.Writer.Length - 1;
                bufferAvailable = Math.Min(this.owner.frameMaxSize, bufferAvailable);

                if (requestedSize == 0 || bufferAvailable > requestedSize)
                {
                    return bufferAvailable;
                }

                this.owner.EndFrame(FrameType.ContentBody, (uint)this.CurrentFrameBytes);
                this.owner.RotateBuffers();
                this.CurrentFrameBytes = 0;

                this.owner.BeginFrame();
                return requestedSize;
            }
        }
    }
}