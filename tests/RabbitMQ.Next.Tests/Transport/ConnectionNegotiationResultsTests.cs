using RabbitMQ.Next.Transport;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport
{
    public class ConnectionNegotiationResultsTests
    {
        [Fact]
        public void CtorTests()
        {
            ushort channelMax = 123;
            uint maxFrameSize = 12345;
            ushort heartbeat = 30;

            var result = new ConnectionNegotiationResults(channelMax, maxFrameSize, heartbeat);

            Assert.Equal(channelMax, result.ChannelMax);
            Assert.Equal(maxFrameSize, result.MaxFrameSize);
            Assert.Equal(heartbeat, result.HeartbeatInterval);
        }
    }
}