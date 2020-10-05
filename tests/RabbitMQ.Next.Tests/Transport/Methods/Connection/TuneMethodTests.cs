using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Connection;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Connection
{
    public class TuneMethodTests
    {
        [Fact]
        public void TuneMethodCtor()
        {
            ushort channelMax = 256;
            uint maxFrameSize = 4096;
            ushort heartbeatInterval = 120;

            var tuneMethod = new TuneMethod(channelMax, maxFrameSize, heartbeatInterval);

            Assert.Equal((uint)MethodId.ConnectionTune, tuneMethod.Method);
            Assert.Equal(channelMax, tuneMethod.ChannelMax);
            Assert.Equal(maxFrameSize, tuneMethod.MaxFrameSize);
            Assert.Equal(heartbeatInterval, tuneMethod.HeartbeatInterval);
        }

        [Fact]
        public void TuneMethodFrameParser()
        {
            var payload = Helpers.GetFileContent("RabbitMQ.Next.Tests.Transport.Methods.Connection.TuneMethodPayload.dat");
            var parser = new TuneMethodParser();
            var data = parser.Parse(payload);
            var dataBoxed = parser.ParseMethod(payload);

            var expected = new TuneMethod(2047, 131072, 60);

            Assert.Equal(expected, data);
            Assert.Equal(expected, dataBoxed);
        }
    }
}