using System;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Connection;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Connection
{
    public class TuneOkMethodTests
    {
        [Fact]
        public void TuneOkMethodCtor()
        {
            ushort channelMax = 256;
            uint maxFrameSize = 4096;
            ushort heartbeatInterval = 120;

            var tuneMethod = new TuneOkMethod(channelMax, maxFrameSize, heartbeatInterval);

            Assert.Equal((uint)MethodId.ConnectionTuneOk, tuneMethod.Method);
            Assert.Equal(channelMax, tuneMethod.ChannelMax);
            Assert.Equal(maxFrameSize, tuneMethod.MaxFrameSize);
            Assert.Equal(heartbeatInterval, tuneMethod.HeartbeatInterval);
        }

        [Fact]
        public void TuneOkMethodFrameFormatter()
        {
            var expected = Helpers.GetFileContent("RabbitMQ.Next.Tests.Transport.Methods.Connection.TuneOkMethodPayload.dat");

            var data = new TuneOkMethod(2047, 131072, 60);
            Span<byte> payload = stackalloc byte[expected.Length];
            new TuneOkMethodFrameFormatter().Write(payload, data);

            Assert.Equal(expected, payload.ToArray());
        }
    }
}