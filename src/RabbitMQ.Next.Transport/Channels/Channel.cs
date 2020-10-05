using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next.Transport.Channels
{
    internal sealed class Channel : IChannel, IDisposable
    {
        private readonly IMethodSender methodSender;
        private readonly WaitMethodFrameHandler waitFrameHandler;
        private readonly RpcMethodHandler rpcHandler;

        private readonly IList<IFrameHandler> methodHandlers;

        public Channel(Pipe pipe, IMethodRegistry methodRegistry, IMethodSender methodSender)
        {
            this.Pipe = pipe;
            this.methodSender = methodSender;
            this.waitFrameHandler = new WaitMethodFrameHandler(methodRegistry);
            this.rpcHandler = new RpcMethodHandler(methodRegistry, this.methodSender);
            this.methodHandlers = new List<IFrameHandler> { this.waitFrameHandler, this.rpcHandler };

            Task.Run(() => this.LoopAsync(pipe.Reader));
        }

        public Pipe Pipe { get; }

        public void Dispose()
        {
            // todo: implement proper resources cleanup
        }

        public Task SendAsync<TMethod>(TMethod request, ReadOnlySequence<byte> content = default)
            where TMethod : struct, IOutgoingMethod =>
            this.methodSender.SendAsync(request, content);

        public Task<TMethod> WaitAsync<TMethod>(CancellationToken cancellation = default)
            where TMethod : struct, IIncomingMethod =>
            this.waitFrameHandler.WaitAsync<TMethod>(cancellation);

        public Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, ReadOnlySequence<byte> content = default, CancellationToken cancellation = default)
            where TRequest : struct, IOutgoingMethod
            where TResponse : struct, IIncomingMethod =>
            this.rpcHandler.SendAsync<TRequest, TResponse>(request, content, cancellation);


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