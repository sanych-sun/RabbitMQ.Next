using System.Buffers;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport
{
    internal interface IFrameSender
    {
        int FrameMaxSize { get; set; }

        ValueTask SendMethodAsync<TMethod>(ushort channelNumber, TMethod method)
            where TMethod : struct, IOutgoingMethod;

        ValueTask SendContentHeaderAsync(ushort channelNumber, MessageProperties properties, ulong contentSize);

        ValueTask SendContentAsync(ushort channelNumber, ReadOnlySequence<byte> contentBytes);
    }
}