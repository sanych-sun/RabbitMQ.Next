using System;
using System.Buffers;
using System.Buffers.Binary;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Transport.Messaging;
using RabbitMQ.Next.Transport.Methods;

namespace RabbitMQ.Next.Transport;

internal class FrameBuilder
{
    private readonly BufferBuilder bufferBuilder;
    private readonly BufferWriter dataWriter;

    private ushort chNumber;
    private int frameMaxSize;

    public FrameBuilder(ObjectPool<MemoryBlock> memoryPool)
    {
        this.bufferBuilder = new BufferBuilder(memoryPool);
        this.dataWriter = new BufferWriter(this);
        this.chNumber = ushort.MaxValue;
    }

    public void Initialize(ushort channelNumber, int frameMaxSize)
    {
        this.chNumber = channelNumber;
        this.frameMaxSize = frameMaxSize;
    }
    
    public void Reset()
    {
        // Should not release memory blocks here! It will be done on the frame sending in Connection.SendLoop
        this.bufferBuilder.Reset();
        this.dataWriter.Reset();
        this.chNumber = ushort.MaxValue;
    }

    public void WriteMethodFrame<TMethod>(TMethod method, IMethodFormatter<TMethod> formatter)
        where TMethod : struct, IOutgoingMethod
    {
        this.dataWriter.BeginFrame(FrameType.Method);
        this.dataWriter.Write((uint)method.MethodId);
        formatter.Write(this.dataWriter, method);
        this.dataWriter.EndFrame();
    }
    
    private const uint ContentHeaderPrefix = (ushort)ClassId.Basic << 16;
    
    public void WriteContentFrame<TState>(TState state, IMessageProperties properties, Action<TState, IBufferWriter<byte>> contentBuilder)
    {
        this.dataWriter.BeginFrame(FrameType.ContentHeader);
    
        this.dataWriter.Write(ContentHeaderPrefix);
        this.dataWriter.Allocate(sizeof(ulong), out var contentSizeBuffer);
        this.dataWriter.WriteMessageProperties(properties);

        this.dataWriter.EndFrame();

        var sizeBeforeContent = this.dataWriter.TotalBytesWritten;
        this.dataWriter.BeginFrame(FrameType.ContentBody);
        contentBuilder.Invoke(state, this.dataWriter);
        this.dataWriter.EndFrame();
        var contentSize = this.dataWriter.TotalBytesWritten - sizeBeforeContent;
        
        BinaryPrimitives.WriteUInt64BigEndian(contentSizeBuffer.Span, (ulong)contentSize);
    }

    public MemoryBlock Complete()
    {
        if (this.dataWriter.CurrentFrameType != FrameType.Malformed)
        {
            throw new InvalidOperationException();
        }

        return this.bufferBuilder.Complete();
    }
    

    private class BufferWriter: IBinaryWriter, IBufferWriter<byte>
    {
        private readonly FrameBuilder builder;
        private FrameType currentFrameType;
        private Memory<byte> frameHeaderBuffer;
        private Memory<byte> buffer;
        private int offset;
        private int currentFrameBytesWritten;
        private int totalBytesWritten;

        public BufferWriter(FrameBuilder builder)
        {
            this.builder = builder;
        }

        public int TotalBytesWritten => this.totalBytesWritten;

        public FrameType CurrentFrameType => this.currentFrameType;

        public void Reset()
        {
            this.currentFrameType = FrameType.Malformed;
            this.frameHeaderBuffer = default;
            this.buffer = default;
            this.offset = 0;
            this.currentFrameBytesWritten = 0;
            this.totalBytesWritten = 0;
        }
        
        public void Advance(int count)
        {
            this.offset += count;
            this.currentFrameBytesWritten += count;
            this.totalBytesWritten += count;
        }

        public Span<byte> GetSpan(int sizeHint)
        {
            var size = this.EnsureBufferSize(sizeHint);
            return this.buffer.Span.Slice( this.offset, size);
        }
    
        public Memory<byte> GetMemory(int sizeHint)
        {
            var size = this.EnsureBufferSize(sizeHint);
            return this.buffer.Slice(this.offset, size);
        }

        public void BeginFrame(FrameType frameType, int payloadMinSize = 0)
        {
            if (this.currentFrameType != FrameType.Malformed)
            {
                throw new InvalidOperationException();
            }

            this.currentFrameType = frameType;
            this.currentFrameBytesWritten = 0;
            this.offset = 0;
            var memory = this.builder.bufferBuilder.GetMemory(ProtocolConstants.FrameHeaderSize + payloadMinSize, ProtocolConstants.FrameHeaderSize + this.builder.frameMaxSize);

            this.frameHeaderBuffer = memory[..ProtocolConstants.FrameHeaderSize];
            this.buffer = memory[ProtocolConstants.FrameHeaderSize..];
        }

        public void EndFrame()
        {
            if (this.currentFrameType == FrameType.Malformed)
            {
                throw new InvalidOperationException();
            }

            if (this.currentFrameBytesWritten > 0)
            {
                Framing.WriteFrameHeader(this.frameHeaderBuffer.Span, this.currentFrameType, this.builder.chNumber, (uint)this.currentFrameBytesWritten);
                this.builder.bufferBuilder.Advance(this.offset + (this.offset == this.currentFrameBytesWritten ? ProtocolConstants.FrameHeaderSize : 0));

                var frameEndBuffer = this.builder.bufferBuilder.GetSpan(ProtocolConstants.FrameEndSize);
                Framing.WriteFrameEnd(frameEndBuffer);
                this.builder.bufferBuilder.Advance(ProtocolConstants.FrameEndSize);
            }

            this.currentFrameType = FrameType.Malformed;
            this.currentFrameBytesWritten = 0;
        }

        private int EnsureBufferSize(int size)
        {
            // TODO: Implement magic to handle the case when body writer requested buffer bigger then buffer size and bigger then frame size  
            
            // 1. Easiest case: current allocated buffer has enough free space
            if (this.buffer.Length - this.offset > size)
            {
                return this.buffer.Length - this.offset;
            }
            
            // 2. Restart frame on new chunk if no data written so far
            if (this.currentFrameBytesWritten == 0)
            {
                var frameType = this.currentFrameType; 
                this.currentFrameType = FrameType.Malformed;
                this.BeginFrame(frameType, size);

                return this.buffer.Length;
            }
            
            // 3. Check if requested bytes fits into the current frame
            if (this.currentFrameBytesWritten + size > this.builder.frameMaxSize)
            {
                if (this.currentFrameType == FrameType.ContentBody)
                {
                    this.EndFrame();
                    this.BeginFrame(FrameType.ContentBody);
                    
                    return this.buffer.Length;
                }
                else
                {
                    throw new OutOfMemoryException($"Cannot extend {this.currentFrameType} frame to fit the payload.");
                }
            }
            
            // 4. Continue the frame on new chunk
            this.builder.bufferBuilder.Advance(this.offset + (this.offset == this.currentFrameBytesWritten ? ProtocolConstants.FrameHeaderSize : 0));
            this.buffer = this.builder.bufferBuilder.GetMemory(size, this.builder.frameMaxSize - this.currentFrameBytesWritten);
            this.offset = 0;

            return this.buffer.Length;
        }
    }
}