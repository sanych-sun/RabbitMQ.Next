using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Transport.Messaging;
using RabbitMQ.Next.Transport.Methods;

namespace RabbitMQ.Next.Transport;

internal class FrameBuilder : IBinaryWriter, IBufferWriter<byte>
{
    private readonly ObjectPool<MemoryBlock> memoryPool;

    private ushort chNumber;
    private int frameMaxSize;
    private MemoryBlock initialBlock;
    private MemoryBlock buffer;
    private int bufferOffset;

    private FrameType currentFrameType;
    private int currentFrameBytesWritten;
    private Memory<byte> currentFrameHeader;
    private int totalBytesWritten;

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
    
    public void Reset()
    {
        // Should not release memory blocks here! It will be done on the frame sending in Connection.SendLoop
        this.initialBlock = default;
        this.buffer = default;
        this.bufferOffset = 0;
        this.chNumber = ushort.MaxValue;
        this.currentFrameType = FrameType.Malformed;
        this.currentFrameHeader = default;
        this.currentFrameBytesWritten = 0;
        this.totalBytesWritten = 0;
    }

    public void WriteMethodFrame<TMethod>(TMethod method, IMethodFormatter<TMethod> formatter)
        where TMethod : struct, IOutgoingMethod
    {
        this.BeginFrame(FrameType.Method);
        this.Write((uint)method.MethodId);
        formatter.Write(this, method);
        this.EndFrame();
    }
    
    private const uint ContentHeaderPrefix = (ushort)ClassId.Basic << 16;
    
    public void WriteContentFrame<TState>(TState state, IMessageProperties properties, Action<TState, IBufferWriter<byte>> contentBuilder)
    {
        this.BeginFrame(FrameType.ContentHeader);
    
        this.Write(ContentHeaderPrefix)
            .Allocate(sizeof(ulong), out var contentSizeBuffer)
            .WriteMessageProperties(properties);

        this.EndFrame();

        var sizeBeforeContent = this.totalBytesWritten;
        this.BeginFrame(FrameType.ContentBody);
        contentBuilder.Invoke(state, this);
        this.EndFrame();
        var contentSize = this.totalBytesWritten - sizeBeforeContent;
        
        BinaryPrimitives.WriteUInt64BigEndian(contentSizeBuffer.Span, (ulong)contentSize);
    }


    // allocates space for generic frame header and returns memory available for payload
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BeginFrame(FrameType frameType)
    {
        if (this.currentFrameType != FrameType.Malformed)
        {
            throw new InvalidOperationException();
        }

        this.currentFrameType = frameType;

        this.EnsureBufferSize(ProtocolConstants.FrameHeaderSize, false);
        this.currentFrameHeader = this.buffer.Memory.Slice(this.bufferOffset, ProtocolConstants.FrameHeaderSize);
        this.bufferOffset += ProtocolConstants.FrameHeaderSize;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EndFrame()
    {
        if (this.currentFrameType == FrameType.Malformed)
        {
            throw new InvalidOperationException();
        }
        
        if (this.currentFrameBytesWritten == 0)
        {
            this.Rollback(ProtocolConstants.FrameHeaderSize);
        }
        else
        {
            Framing.WriteFrameHeader(this.currentFrameHeader.Span, this.currentFrameType, this.chNumber, (uint)this.currentFrameBytesWritten);

            Framing.WriteFrameEnd(this.buffer.Memory[this.bufferOffset..]);
            this.bufferOffset += ProtocolConstants.FrameEndSize;
        }

        this.currentFrameType = FrameType.Malformed;
        this.currentFrameBytesWritten = 0;
    }

    private void Rollback(int count)
    {
        this.bufferOffset -= count;
    }

    public MemoryBlock Complete()
    {
        if (this.currentFrameType != FrameType.Malformed)
        {
            throw new InvalidOperationException();
        }

        this.CompleteCurrentBuffer();
        return this.initialBlock;
    }

    Memory<byte> IBufferWriter<byte>.GetMemory(int sizeHint) => this.GetMemory(sizeHint);

    Span<byte> IBufferWriter<byte>.GetSpan(int sizeHint) => this.GetSpan(sizeHint);

    void IBufferWriter<byte>.Advance(int count) => this.Advance(count);

    Memory<byte> IBinaryWriter.GetMemory(int sizeHint) => this.GetMemory(sizeHint);
    
    Span<byte> IBinaryWriter.GetSpan(int sizeHint) => this.GetSpan(sizeHint);

    void IBinaryWriter.Advance(int count) => this.Advance(count);
    
    int IBinaryWriter.BytesWritten => this.totalBytesWritten;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Advance(int count)
    {
        this.bufferOffset += count;

        this.currentFrameBytesWritten += count;
        this.totalBytesWritten += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Span<byte> GetSpan(int sizeHint)
    {
        var size = this.EnsureBufferSize(sizeHint);
        return this.buffer.Memory.Slice(this.bufferOffset, size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Memory<byte> GetMemory(int sizeHint)
    {
        var size = this.EnsureBufferSize(sizeHint);
        return this.buffer.Memory.Slice(this.bufferOffset, size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EnsureBufferSize(int requestedSize, bool validateFrame = true)
    {
        if (validateFrame && this.currentFrameBytesWritten + requestedSize > this.frameMaxSize)
        {
            if (this.currentFrameType != FrameType.ContentBody)
            {
                throw new OutOfMemoryException();
            }
            
            this.EndFrame();
            this.BeginFrame(FrameType.ContentBody);
        }

        if (this.bufferOffset + requestedSize + ProtocolConstants.FrameEndSize > this.buffer.Memory.Count)
        {
            if (this.currentFrameType != FrameType.Malformed && this.currentFrameBytesWritten == 0)
            {
                this.Rollback(ProtocolConstants.FrameHeaderSize);
            }
            
            this.CompleteCurrentBuffer();
            
            // todo: refactor to allow allocation of chunks bigger then configured buffer size 
            var nextBuffer = this.memoryPool.Get();
            this.buffer.Next = nextBuffer;
            this.buffer = nextBuffer;
            this.bufferOffset = 0;
        }

        return Math.Min(this.frameMaxSize - this.currentFrameBytesWritten, this.buffer.Memory.Count - this.bufferOffset - ProtocolConstants.FrameEndSize);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CompleteCurrentBuffer()
    {
        this.buffer.Slice(this.bufferOffset);
    }
}