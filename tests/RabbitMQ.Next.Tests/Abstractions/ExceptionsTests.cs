using System;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Exceptions;
using Xunit;

namespace RabbitMQ.Next.Tests.Abstractions
{
    public class ExceptionsTests
    {
        [Fact]
        public void ChannelException()
        {
            var errorCode = (ushort) 404;
            var description = "not found";
            var failedMethodId = MethodId.BasicDeliver;

            var ex = new ChannelException(errorCode, description, failedMethodId);

            Assert.Equal(errorCode, ex.ErrorCode);
            Assert.Equal(description, ex.Message);
            Assert.Equal(failedMethodId, ex.FailedMethodId);
        }


        [Fact]
        public void ConnectionException()
        {
            var errorCode = (ushort) 404;
            var description = "not found";

            var ex = new ConnectionException(errorCode, description);

            Assert.Equal(errorCode, ex.ErrorCode);
            Assert.Equal(description, ex.Message);
        }

        [Fact]
        public void EndPointResolutionException()
        {
            var endpoint = new Uri("ampq://localhost:8182/");
            var inner = new Exception();

            var ex = new EndPointResolutionException(endpoint, inner);

            Assert.Equal(endpoint, ex.Endpoint);
            Assert.Equal(inner, ex.InnerException);
        }
    }
}