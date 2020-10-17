using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next.Transport.Channels
{
    internal class FrameSender : IFrameSender
    {
        private readonly ISocketWriter socketWriter;
        private readonly IMethodRegistry registry;
        private readonly ushort channelNumber;
        // TODO: use array pool instead
        private readonly Memory<byte> writerBuffer;

        public FrameSender(ISocketWriter socketWriter, IMethodRegistry registry, ushort channelNumber, int maxFrameSize = ProtocolConstants.FrameMinSize)
        {
            this.socketWriter = socketWriter;
            this.registry = registry;
            this.channelNumber = channelNumber;
            this.writerBuffer = new byte[maxFrameSize + ProtocolConstants.FrameHeaderSize];
        }

        public Task SendMethodAsync<TMethod>(TMethod method)
            where TMethod : struct, IOutgoingMethod =>
            this.SendMethodFrameInternalAsync(method);

        private Task SendMethodFrameInternalAsync<TMethod>(TMethod method)
            where TMethod : struct, IOutgoingMethod
        {
            var formatter = this.registry.GetFormatter<TMethod>();
            if (formatter == null)
            {
                // TODO: throw connection-level exception here?
                throw new InvalidOperationException();
            }

            // TODO: implement functionality to extend buffer if connection configured to use bigger frames
            // and data does not fit into default framesize
            var payload = this.writerBuffer.Span.Slice(ProtocolConstants.FrameHeaderSize);
            var initialSize = payload.Length;
            payload = payload.Write(method.Method);
            payload = formatter.Write(payload, method);

            var payloadSize = initialSize - payload.Length; // have to add size of methodId
            this.writerBuffer.Span.Slice(0, ProtocolConstants.FrameHeaderSize)
                .WriteFrameHeader(new FrameHeader(FrameType.Method, this.channelNumber, payloadSize));

            this.writerBuffer.Span.Slice(ProtocolConstants.FrameHeaderSize + payloadSize);

            var data = this.writerBuffer.Slice(0, ProtocolConstants.FrameHeaderSize + payloadSize);

            return this.socketWriter.SendAsync(data);
        }
    }
}