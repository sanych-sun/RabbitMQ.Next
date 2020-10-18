using RabbitMQ.Next.Abstractions.Exceptions;
using Xunit;

namespace RabbitMQ.Next.Tests.Abstractions.Exceptions
{
    public class ChannelExceptionTests
    {
        [Fact]
        public void ChannelExceptionCtor()
        {
            var errorCode = 404;
            var description = "not found";
            var failedMethodId = (uint)42;

            var ex = new ChannelException(errorCode, description, failedMethodId);

            Assert.Equal(errorCode, ex.ErrorCode);
            Assert.Equal(description, ex.Message);
            Assert.Equal(failedMethodId, ex.FailedMethodId);
        }
    }
}