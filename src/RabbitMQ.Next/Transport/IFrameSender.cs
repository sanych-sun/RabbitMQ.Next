using System.Buffers;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport
{
    internal interface IFrameSender
    {
        ValueTask SendHeartBeatAsync();

        ValueTask SendMethodAsync<TMethod>(ushort channelNumber, TMethod method)
            where TMethod : struct, IOutgoingMethod;

        ValueTask SendContentHeaderAsync(ushort channelNumber, IMessageProperties properties, ulong contentSize);

        ValueTask SendContentAsync(ushort channelNumber, ReadOnlySequence<byte> contentBytes);
    }
}