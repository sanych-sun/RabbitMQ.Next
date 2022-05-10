using System;
using System.Collections.Generic;
using RabbitMQ.Next.Exceptions;
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

            var ex = new EndPointResolutionException(new Dictionary<Uri, Exception>{ [endpoint] = inner });

            Assert.True(ex.InnerExceptions.ContainsKey(endpoint));
            Assert.Equal(inner, ex.InnerExceptions[endpoint]);
        }
    }
}