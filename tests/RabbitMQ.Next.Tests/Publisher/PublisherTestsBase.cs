using System.Buffers;
using System.Collections.Generic;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Publisher.Transformers;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport.Buffers;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Tests.Publisher
{
    public class PublisherTestsBase
    {
        public static IEnumerable<object[]> PublishTestCases()
        {
            yield return new object[]
            {
                null,
                "myExchange", "key", null, PublishFlags.None,
                new PublishMethod("myExchange", "key", 0),
                null
            };

            yield return new object[]
            {
                null,
                "myExchange", "key", null, PublishFlags.Immediate,
                new PublishMethod("myExchange", "key", (byte)PublishFlags.Immediate),
                null
            };

            yield return new object[]
            {
                null,
                "myExchange", "key", new MessageProperties { ApplicationId = "test"}, PublishFlags.None,
                new PublishMethod("myExchange", "key", 0),
                new MessageProperties { ApplicationId = "test"}
            };

            yield return new object[]
            {
                new IMessageTransformer[] { new ExchangeTransformer("default")},
                "", "key", null, PublishFlags.None,
                new PublishMethod("default", "key", 0),
                new MessageProperties()
            };

            yield return new object[]
            {
                null,
                "myExchange", "key", new MessageProperties { Priority = 1, Type = "test"}, PublishFlags.None,
                new PublishMethod("myExchange", "key", 0),
                new MessageProperties { Priority = 1, Type = "test"}
            };

            yield return new object[]
            {
                new IMessageTransformer[] { new UserIdTransformer("testUser")},
                "exchange", "key", new MessageProperties { Priority = 1, Type = "test"}, PublishFlags.None,
                new PublishMethod("exchange", "key", 0),
                new MessageProperties { Priority = 1, Type = "test", UserId = "testUser"}
            };
        }

        protected ISerializer MockSerializer()
        {
            var serializer = Substitute.For<ISerializer>();
            serializer
                .When(x => x.Serialize(Arg.Any<string>(), Arg.Any<IBufferWriter>()))
                .Do(x => x.ArgAt<IBufferWriter>(1).Write(new byte[] { 0x01, 0x02}));

            return serializer;
        }

        protected IConnection MockConnection()
        {
            var connection = Substitute.For<IConnection>();
            connection.BufferPool.Returns(new BufferPool(1024));
            connection.State.Returns(ConnectionState.Open);

            return connection;
        }
    }
}