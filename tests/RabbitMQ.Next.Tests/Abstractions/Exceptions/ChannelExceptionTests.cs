using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Exceptions;
using Xunit;

namespace RabbitMQ.Next.Tests.Abstractions.Exceptions
{
    public class ChannelExceptionTests
    {
        [Fact]
        public void ChannelExceptionCtor()
        {
            var errorCode = (ushort)404;
            var description = "not found";
            var failedMethodId = MethodId.BasicDeliver;

            var ex = new ChannelException(errorCode, description, failedMethodId);

            Assert.Equal(errorCode, ex.ErrorCode);
            Assert.Equal(description, ex.Message);
            Assert.Equal(failedMethodId, ex.FailedMethodId);
        }
    }
}