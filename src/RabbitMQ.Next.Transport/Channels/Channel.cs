using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next.Transport.Channels
{
    internal sealed class Channel : IChannel, IDisposable
    {
        private readonly IFrameSender frameSender;
        private readonly IMethodRegistry methodRegistry;
        private readonly WaitMethodFrameHandler waitFrameHandler;
        private readonly SemaphoreSlim senderSync;

        private readonly IList<IFrameHandler> methodHandlers;

        public Channel(Pipe pipe, IMethodRegistry methodRegistry, IFrameSender frameSender)
        {
            this.Pipe = pipe;
            this.frameSender = frameSender;
            this.methodRegistry = methodRegistry;
            this.waitFrameHandler = new WaitMethodFrameHandler(methodRegistry);
            this.methodHandlers = new List<IFrameHandler> { this.waitFrameHandler };
            this.senderSync = new SemaphoreSlim(1,1);

            Task.Run(() => this.LoopAsync(pipe.Reader));
        }

        public Pipe Pipe { get; }

        public void Dispose()
        {
            // todo: implement proper resources cleanup
        }

        public bool IsClosed { get; }

        public async Task SendAsync<TMethod>(TMethod request, ReadOnlySequence<byte> content = default)
            where TMethod : struct, IOutgoingMethod
        {
            await this.senderSync.WaitAsync();

            try
            {
                await this.frameSender.SendMethodAsync(request);
            }
            finally
            {
                this.senderSync.Release();
            }
        }

        public async Task<TMethod> WaitAsync<TMethod>(CancellationToken cancellation = default)
            where TMethod : struct, IIncomingMethod
        {
            var expectedId = this.methodRegistry.GetMethodId<TMethod>();
            var result = await this.waitFrameHandler.WaitAsync(expectedId, cancellation);
            return (TMethod) result;
        }

        public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, ReadOnlySequence<byte> content = default)
            where TRequest : struct, IOutgoingMethod
            where TResponse : struct, IIncomingMethod
        {
            var expectedId = this.methodRegistry.GetMethodId<TResponse>();
            await this.senderSync.WaitAsync();

            try
            {
                await this.frameSender.SendMethodAsync(request);

                var result = await this.waitFrameHandler.WaitAsync(expectedId);
                return (TResponse) result;
            }
            finally
            {
                this.senderSync.Release();
            }
        }

        private async Task LoopAsync(PipeReader pipeReader)
        {
            Func<ReadOnlySequence<byte>, FrameHeader> headerParser = this.ParseFrameHeader;
            Func<ReadOnlySequence<byte>, bool> methodFrameHandler = this.ParseMethodFramePayload;

            while (true)
            {
                var header = await pipeReader.ReadAsync(ProtocolConstants.FrameHeaderSize, headerParser);
                if (header.IsEmpty())
                {
                    return;
                }

                Func<ReadOnlySequence<byte>, bool> payloadHandler;
                switch (header.Type)
                {
                    case FrameType.Method:
                        payloadHandler = methodFrameHandler;
                        break;
                    default:
                        // todo: throw connection exception here for unexpected frame
                        throw new InvalidOperationException();
                }

                await pipeReader.ReadAsync(header.PayloadSize, payloadHandler);
            }
        }

        private bool ParseMethodFramePayload(ReadOnlySequence<byte> sequence) => this.ParseFramePayload(FrameType.Method, sequence);

        private bool ParseFramePayload(FrameType type, ReadOnlySequence<byte> sequence)
        {
            for (var i = 0; i < this.methodHandlers.Count; i++)
            {
                if (this.methodHandlers[i].Handle(type, sequence))
                {
                    return true;
                }
            }

            return false;
        }

        private FrameHeader ParseFrameHeader(ReadOnlySequence<byte> sequence)
        {
            if (sequence.IsSingleSegment)
            {
                return sequence.FirstSpan.ReadFrameHeader();
            }

            Span<byte> headerBuffer = stackalloc byte[ProtocolConstants.FrameHeaderSize];
            sequence.Slice(0, ProtocolConstants.FrameHeaderSize).CopyTo(headerBuffer);
            return ((ReadOnlySpan<byte>) headerBuffer).ReadFrameHeader();
        }
    }
}