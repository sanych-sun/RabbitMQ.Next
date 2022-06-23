using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Messaging;

namespace RabbitMQ.Next.Channels
{
    internal class FrameBuilder : IBufferWriter<byte>
    {
        private readonly ObjectPool<MemoryBlock> memoryPool;
        private ushort chNumber;
        private int frameMaxSize;
        private MemoryBlock initialBlock;
        private MemoryBlock buffer;
        private int currentFrameHeaderOffset;
        private int totalPayloadSize;

        public FrameBuilder(ObjectPool<MemoryBlock> memoryPool)
        {
            this.memoryPool = memoryPool;
            this.chNumber = ushort.MaxValue;
        }

        public void Initialize(ushort channelNumber, int frameMaxSize)
        {
            this.chNumber = channelNumber;
            this.frameMaxSize = frameMaxSize;
            this.buffer = this.memoryPool.Get();
            this.initialBlock = this.buffer;
        }

        public void WriteMethodFrame<TMethod>(TMethod method, IMethodFormatter<TMethod> formatter)
            where TMethod : struct, IOutgoingMethod
        {
            this.BeginFrame();
            var payloadBuffer = this.buffer.Span
                .Write((uint)method.MethodId);

            var payloadSize = formatter.Write(payloadBuffer, method) + sizeof(uint);
            this.buffer.Commit(payloadSize);
            this.EndFrame(FrameType.Method);
        }

        private const uint ContentHeaderPrefix = (ushort)ClassId.Basic << 16;

        public void WriteContentFrame<TState>(TState state, IMessageProperties properties, Action<TState, IBufferWriter<byte>> contentBuilder)
        {
            this.BeginFrame();

            this.buffer.Span.Write(ContentHeaderPrefix);
            this.buffer.Commit(sizeof(uint));

            var contentSizeBuffer = this.buffer.Span[..sizeof(ulong)];
            this.buffer.Commit(sizeof(ulong));

            var written = this.buffer.Span.WriteMessageProperties(properties);
            this.buffer.Commit(written);

            this.EndFrame(FrameType.ContentHeader);

            var beforeContentOffset = this.buffer.Offset;
            var beforeContent = this.totalPayloadSize;
            this.BeginFrame();
            contentBuilder.Invoke(state, this);
            this.EndFrame(FrameType.ContentBody);
            var contentSize = this.totalPayloadSize - beforeContent;

            if (contentSize == 0)
            {
                this.buffer.Rollback(beforeContentOffset);
            }

            contentSizeBuffer.Write((ulong)contentSize);
        }


        // allocates space for generic frame header and returns memory available for payload
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BeginFrame()
        {
            this.currentFrameHeaderOffset = this.buffer.Offset;
            // skip frame header bytes for now, will write it in EndFrame.
            this.buffer.Commit(ProtocolConstants.FrameHeaderSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EndFrame(FrameType type, bool rotateBuffers = false)
        {
            var frameSize = this.buffer.Offset - this.currentFrameHeaderOffset - ProtocolConstants.FrameHeaderSize;
            var frameHeader = this.buffer.Access(this.currentFrameHeaderOffset, ProtocolConstants.FrameHeaderSize);
            frameHeader.WriteFrameHeader(type, this.chNumber, (uint)frameSize);

            this.buffer.Span[0] = ProtocolConstants.FrameEndByte;
            this.buffer.Commit(1);
            this.totalPayloadSize += frameSize;

            if (!rotateBuffers)
            {
                return;
            }

            this.buffer = this.buffer.Append(this.memoryPool.Get());
        }

        public MemoryBlock Complete()
            => this.initialBlock;

        public void Reset()
        {
            // Should not release memory blocks here! It will be done on the frame sending in Connection.SendLoop
            this.initialBlock = default;
            this.buffer = default;
            this.chNumber = ushort.MaxValue;
            this.currentFrameHeaderOffset = 0;
            this.totalPayloadSize = 0;
        }


        void IBufferWriter<byte>.Advance(int count)
        {
            this.buffer.Commit(count);
        }

        public Memory<byte> GetMemory(int sizeHint)
        {
            var size = this.ExpandIfRequired(sizeHint);
            return this.buffer.Memory[..size];
        }

        public Span<byte> GetSpan(int sizeHint)
        {
            var size = this.ExpandIfRequired(sizeHint);
            return this.buffer.Span[..size];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ExpandIfRequired(int requestedSize)
        {
            // todo: implement workaround for too big chunks by allocating some extra array with merging the data back into buffer on Advance
            if (requestedSize > this.frameMaxSize)
            {
                throw new OutOfMemoryException();
            }

            // current buffer available space: capacity - frame end
            var bufferAvailable = this.buffer.Span.Length - 1;
            bufferAvailable = Math.Min(this.frameMaxSize, bufferAvailable);

            if (requestedSize == 0 || bufferAvailable > requestedSize)
            {
                return bufferAvailable;
            }

            this.EndFrame(FrameType.ContentBody, true);

            this.BeginFrame();
            return requestedSize;
        }
    }
}