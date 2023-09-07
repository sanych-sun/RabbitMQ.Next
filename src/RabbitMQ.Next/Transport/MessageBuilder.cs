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
    private readonly int maxFrameSize;

    public MessageBuilder(ObjectPool<byte[]> memoryPool, ushort channel, int maxFrameSize)
    {
        this.Channel = channel;
        this.maxFrameSize = maxFrameSize;
        this.writer = new InnerBufferWriter(memoryPool, channel, maxFrameSize);
    }
    
    public ushort Channel { get; }
    
    public int MaxFrameSize => this.maxFrameSize;

    public void WriteMethodFrame<TMethod>(TMethod method)
        where TMethod : struct, IOutgoingMethod
    {
        this.writer.BeginFrame(FrameType.Method);
        var buffer = this.writer.GetSpan();

        var result = buffer.WriteMethodArgs(method);
        this.writer.Advance(buffer.Length - result.Length);
        
        this.writer.EndFrame();
    }

    private const uint ContentHeaderPrefix = (ushort)ClassId.Basic << 16;
    
    public void WriteContentFrame<TState>(TState state, IMessageProperties properties, Action<TState, IBufferWriter<byte>> contentBuilder)
    {
        this.writer.BeginFrame(FrameType.ContentHeader);
        var headerStartBuffer = this.writer.GetSpan();

        var headerBuffer = headerStartBuffer.Write(ContentHeaderPrefix);
        var contentSizeBuffer = headerBuffer[..sizeof(ulong)];
        headerBuffer = headerBuffer[sizeof(ulong)..];

        headerBuffer = headerBuffer.WriteContentProperties(properties);
        
        this.writer.Advance(headerStartBuffer.Length - headerBuffer.Length);
        this.writer.EndFrame();

        var beforeContentSize = this.writer.TotalPayloadBytes;
        
        this.writer.BeginFrame(FrameType.ContentBody);
        contentBuilder.Invoke(state, this.writer);
        this.writer.EndFrame();
        var contentSize = this.writer.TotalPayloadBytes - beforeContentSize;
        
        contentSizeBuffer.Write((ulong)contentSize);
    }
    
    public IMemoryAccessor Complete()
        => this.writer.Complete();

    public void Reset()
    {
        this.writer.Reset();
    }

    private class InnerBufferWriter : IBufferWriter<byte>
    {
        private readonly ObjectPool<byte[]> memoryPool;
        private readonly ushort channel;
        private readonly int frameMaxSize;
        
        private IMemoryAccessor head;
        private IMemoryAccessor tail;
        private byte[] buffer;
        private int bufferOffset;
        private FrameType frameType;
        private int frameHeaderOffset;
        private long totalPayloadSize;

        public InnerBufferWriter(ObjectPool<byte[]> memoryPool, ushort channel, int frameMaxSize)
        {
            this.memoryPool = memoryPool;
            this.channel = channel;
            this.frameMaxSize = frameMaxSize;
        }

        public long TotalPayloadBytes => this.totalPayloadSize;
        
        public void BeginFrame(FrameType type)
        {
            if(this.frameType != FrameType.None)
            {
                throw new InvalidOperationException();
            }

            this.frameHeaderOffset = this.bufferOffset;
            this.frameType = type;
            this.bufferOffset += ProtocolConstants.FrameHeaderSize;
        }

        public IMemoryAccessor Complete()
        {
            if (this.frameType != FrameType.None)
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
            this.tail = null;
            this.head = null;
            this.buffer = null;
            this.bufferOffset = 0;
            this.frameType = FrameType.None;
            this.frameHeaderOffset = 0;
            this.totalPayloadSize = 0;
        }

        public void EndFrame()
        {
            if (this.frameType == FrameType.None)
            {
                throw new InvalidOperationException();
            }
            
            var frameSize = this.bufferOffset - this.frameHeaderOffset - ProtocolConstants.FrameHeaderSize;
            if (frameSize == 0)
            {
                if (this.frameType != FrameType.ContentBody)
                {
                    throw new InvalidOperationException();
                }
                
                // collapse empty content frames
                this.bufferOffset = this.frameHeaderOffset;
                this.frameType = FrameType.None;
                return;
            }
            
            var headerBuffer = new Span<byte>(this.buffer, this.frameHeaderOffset, ProtocolConstants.FrameHeaderSize);
            headerBuffer.WriteFrameHeader(this.frameType, this.channel, (uint)frameSize);

            var endBuffer = new Span<byte>(this.buffer, this.bufferOffset, ProtocolConstants.FrameEndSize);
            endBuffer.WriteFrameEnd();
            this.bufferOffset += ProtocolConstants.FrameEndSize;

            this.totalPayloadSize += frameSize;
            this.frameType = FrameType.None;
        }

        public void Advance(int count)
        {
            this.bufferOffset += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            var size = this.EnsureBufferSize(sizeHint);
            return new Memory<byte>(this.buffer, this.bufferOffset, size);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            var size = this.EnsureBufferSize(sizeHint);
            return new Span<byte>(this.buffer, this.bufferOffset, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EnsureBufferSize(int requestedSize)
        {
            // current buffer available space: capacity - offset - frame end
            int BufferAvailable()
                => this.buffer.Length - this.bufferOffset - ProtocolConstants.FrameEndSize;

            // todo: implement workaround for too big chunks by allocating some extra array with merging the data back into buffer on Advance
            if (requestedSize > this.frameMaxSize)
            {
                throw new OutOfMemoryException();
            }

            this.buffer ??= this.memoryPool.Get();

            var size = Math.Min(this.frameMaxSize, BufferAvailable());
            if (size > 0 && (requestedSize == 0 || size > requestedSize))
            {
                return size;
            }

            if (this.frameType != FrameType.ContentBody)
            {
                throw new OutOfMemoryException();
            }
            
            this.EndFrame();
            this.FinalizeBuffer();
            this.EnsureBufferSize(requestedSize + ProtocolConstants.FrameHeaderSize);
            this.BeginFrame(FrameType.ContentBody);
            
            return Math.Min(this.frameMaxSize, BufferAvailable());
        }

        private void FinalizeBuffer()
        {
            var memoryAccessor = new PooledMemoryAccessor(this.memoryPool, this.buffer, 0, this.bufferOffset);
            this.buffer = null;
            this.bufferOffset = 0;

            if (this.tail == null)
            {
                this.head = memoryAccessor;
                this.tail = memoryAccessor;
            }
            else
            {
                this.tail = this.tail.Append(memoryAccessor);
            }
        }
    }
}