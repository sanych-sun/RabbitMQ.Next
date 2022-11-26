using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport;

internal class MessageBuilder
{
    private readonly InnerBufferWriter writer;

    public MessageBuilder(ObjectPool<MemoryBlock> memoryPool, ushort channel, int frameMaxSize)
    {
        this.Channel = channel;
        this.FrameMaxSize = frameMaxSize;
        this.writer = new InnerBufferWriter(memoryPool, channel, frameMaxSize);
    }

    public ushort Channel { get; }

    public int FrameMaxSize { get; }

    public void WriteMethodFrame<TMethod>(TMethod method)
        where TMethod : struct, IOutgoingMethod
    {
        this.writer.BeginFrame(FrameType.Method);
        var buffer = this.writer.GetSpan();

        var result = buffer.WriteMethodArgs(method);
        this.writer.Advance(buffer.Length - result.Length);
        
        this.writer.EndFrame();
    }

    public void WriteContentFrame<TState>(TState state, IMessageProperties properties, Action<TState, IBufferWriter<byte>> contentBuilder)
    {
        this.writer.BeginFrame(FrameType.ContentHeader);
        var headerStartBuffer = this.writer.GetSpan();

        var headerBuffer = headerStartBuffer.WriteContentHeader(properties, out var contentSizeBuffer);
        
        this.writer.Advance(headerStartBuffer.Length - headerBuffer.Length);
        this.writer.EndFrame();

        var beforeContentSize = this.writer.TotalPayloadBytes;
        
        this.writer.BeginFrame(FrameType.ContentBody);
        contentBuilder.Invoke(state, this.writer);
        this.writer.EndFrame();
        var contentSize = this.writer.TotalPayloadBytes - beforeContentSize;
        
        contentSizeBuffer.Write((ulong)contentSize);
    }
    
    public MemoryBlock Complete()
        => this.writer.Complete();

    public void Reset()
    {
        this.writer.Reset();
    }

    private class InnerBufferWriter : IBufferWriter<byte>
    {
        private readonly ObjectPool<MemoryBlock> memoryPool;
        private readonly ushort channel;
        private readonly int frameMaxSize;
        
        private MemoryBlock head;
        private MemoryBlock current;
        private int currentOffset;
        private FrameType currentFrameType;
        private int currentFrameHeaderOffset;
        private long totalPayloadSize;

        public InnerBufferWriter(ObjectPool<MemoryBlock> memoryPool, ushort channel, int frameMaxSize)
        {
            this.memoryPool = memoryPool;
            this.channel = channel;
            this.frameMaxSize = frameMaxSize;
        }

        public long TotalPayloadBytes => this.totalPayloadSize;
        
        public void BeginFrame(FrameType type)
        {
            if(this.currentFrameType != FrameType.Malformed)
            {
                throw new InvalidOperationException();
            }

            this.currentFrameHeaderOffset = this.currentOffset;
            this.currentFrameType = type;
            this.currentOffset += ProtocolConstants.FrameHeaderSize;
        }

        public MemoryBlock Complete()
        {
            if (this.currentFrameType != FrameType.Malformed)
            {
                throw new InvalidOperationException();
            }
            
            this.FinalizeBuffer();
            var result = this.head;
            this.Reset();
            return result;
        }

        public void Reset()
        {
            this.head = null;
            this.current = null;
            this.currentOffset = 0;
            this.currentFrameHeaderOffset = 0;
            this.totalPayloadSize = 0;
        }

        public void EndFrame()
        {
            if (this.currentFrameType == FrameType.Malformed)
            {
                throw new InvalidOperationException();
            }
            
            var frameSize = this.currentOffset - this.currentFrameHeaderOffset - ProtocolConstants.FrameHeaderSize;
            if (frameSize == 0)
            {
                if (this.currentFrameType != FrameType.ContentBody)
                {
                    throw new InvalidOperationException();
                }
                
                // collapse empty content frames
                this.currentOffset = this.currentFrameHeaderOffset;
                this.currentFrameType = FrameType.Malformed;
                return;
            }
            
            var headerBuffer = new Span<byte>(this.current.Buffer, this.currentFrameHeaderOffset, ProtocolConstants.FrameHeaderSize);
            headerBuffer.WriteFrameHeader(this.currentFrameType, this.channel, (uint)frameSize);

            var endBuffer = new Span<byte>(this.current.Buffer, this.currentOffset, ProtocolConstants.FrameEndSize);
            endBuffer.WriteFrameEnd();
            this.currentOffset += ProtocolConstants.FrameEndSize;

            this.totalPayloadSize += frameSize;
            this.currentFrameType = FrameType.Malformed;
        }

        public void Advance(int count)
        {
            this.currentOffset += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            var size = this.EnsureBufferSize(sizeHint);
            return new Memory<byte>(this.current.Buffer, this.currentOffset, size);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            var size = this.EnsureBufferSize(sizeHint);
            return new Span<byte>(this.current.Buffer, this.currentOffset, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EnsureBufferSize(int requestedSize)
        {
            // current buffer available space: capacity - offset - frame end
            int BufferAvailable()
                => this.current.Buffer.Length - this.currentOffset - ProtocolConstants.FrameEndSize;

            // todo: implement workaround for too big chunks by allocating some extra array with merging the data back into buffer on Advance
            if (requestedSize > this.frameMaxSize)
            {
                throw new OutOfMemoryException();
            }

            if (this.current == null)
            {
                this.current = this.memoryPool.Get();
                this.head = this.current;
            }

            var size = Math.Min(this.frameMaxSize, BufferAvailable());
            if (requestedSize == 0 || size > requestedSize)
            {
                return size;
            }

            if (this.currentFrameType != FrameType.ContentBody)
            {
                throw new OutOfMemoryException();
            }
            
            this.EndFrame();
            this.FinalizeBuffer(true);
            this.BeginFrame(FrameType.ContentBody);
            
            return Math.Min(this.frameMaxSize, BufferAvailable());
        }

        private void FinalizeBuffer(bool startNew = false)
        {
            this.current.Slice(0, this.currentOffset);

            if (startNew)
            {
                var next = this.memoryPool.Get();
                this.current = this.current.Append(next);
                this.currentOffset = 0;
            }
        }
    }
}