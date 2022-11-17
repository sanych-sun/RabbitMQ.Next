using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Transport.Messaging;
using RabbitMQ.Next.Transport.Methods;

namespace RabbitMQ.Next.Transport;

internal class MessageBuilder : IBufferWriter<byte>
{
    private readonly ObjectPool<MemoryBlock> memoryPool;
    private MemoryBlock head;
    private MemoryBlock buffer;
    private int currentFrameHeaderOffset;
    private int totalPayloadSize;

    public MessageBuilder(ObjectPool<MemoryBlock> memoryPool, ushort channel, int frameMaxSize)
    {
        this.memoryPool = memoryPool;
        this.Channel = channel;
        this.FrameMaxSize = frameMaxSize;
    }
    
    public ushort Channel { get; }
    
    public int FrameMaxSize { get; }

    public void WriteMethodFrame<TMethod>(TMethod method)
        where TMethod : struct, IOutgoingMethod
    {
        this.BeginFrame();
        var payloadBuffer = this.buffer.Span
            .Write((uint)method.MethodId);

        var formatter = MethodRegistry.GetFormatter<TMethod>();
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
        this.ExpandIfRequired(ProtocolConstants.FrameHeaderSize);
        
        this.currentFrameHeaderOffset = this.buffer.Offset;
        // skip frame header bytes for now, will write it in EndFrame.
        this.buffer.Commit(ProtocolConstants.FrameHeaderSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EndFrame(FrameType type, bool rotateBuffers = false)
    {
        var frameSize = this.buffer.Offset - this.currentFrameHeaderOffset - ProtocolConstants.FrameHeaderSize;
        var frameHeader = this.buffer.Access(this.currentFrameHeaderOffset, ProtocolConstants.FrameHeaderSize);
        frameHeader.WriteFrameHeader(type, this.Channel, (uint)frameSize);

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
        => this.head;

    public void Reset()
    {
        // Should not release memory blocks here! It will be done on the frame sending in Connection.SendLoop
        this.head = default;
        this.buffer = default;
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
        if (requestedSize > this.FrameMaxSize)
        {
            throw new OutOfMemoryException();
        }

        if (this.buffer == null)
        {
            this.buffer = this.memoryPool.Get();
            this.head = this.buffer;
        }

        // current buffer available space: capacity - frame end
        var bufferAvailable = this.buffer.Span.Length - 1;
        bufferAvailable = Math.Min(this.FrameMaxSize, bufferAvailable);

        if (requestedSize == 0 || bufferAvailable > requestedSize)
        {
            return bufferAvailable;
        }

        this.EndFrame(FrameType.ContentBody, true);

        this.BeginFrame();
        return requestedSize;
    }
}