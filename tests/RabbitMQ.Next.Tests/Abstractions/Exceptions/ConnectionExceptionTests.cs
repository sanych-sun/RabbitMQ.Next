using RabbitMQ.Next.Abstractions.Exceptions;
using Xunit;

namespace RabbitMQ.Next.Tests.Abstractions.Exceptions
{
    public class ConnectionExceptionTests
    {
        [Fact]
        public void ConnectionExceptionCtor()
        {
            var errorCode = (ushort)404;
            var description = "not found";

            var ex = new ConnectionException(errorCode, description);

            Assert.Equal(errorCode, ex.ErrorCode);
            Assert.Equal(description, ex.Message);
        }
    }
}