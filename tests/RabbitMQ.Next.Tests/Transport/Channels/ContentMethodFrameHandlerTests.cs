using System;
using System.Buffers;
using System.Collections.Generic;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Transport.Buffers;
using RabbitMQ.Next.Transport.Channels;
using RabbitMQ.Next.Transport.Methods.Basic;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Channels
{
    public class ContentMethodFrameHandlerTests
    {
        [Theory]
        [MemberData(nameof(ValidFramesTestCases))]
        public void ShouldHandleValidFrames(DeliverMethod deliver, IMessageProperties properties, byte[] content, (ChannelFrameType type, ReadOnlySequence<byte> data)[] frames)
        {
            DeliverMethod receivedDeliver = default;
            IMessageProperties receivedProperties = null;
            byte[] receivedContent = null;

            var frameHandler = this.Create((d, p, c) =>
            {
                receivedDeliver = d;
                receivedProperties = p;
                receivedContent = c.ToArray();
            });

            foreach (var frame in frames)
            {
                ((IFrameHandler) frameHandler).Handle(frame.type, frame.data);
            }

            Assert.Equal(deliver, receivedDeliver);
            Assert.Equal(properties.MessageId, receivedProperties.MessageId);
            Assert.Equal(content, receivedContent);
        }

        [Theory]
        [MemberData(nameof(IgnoreFramesTestCases))]
        public void ShouldIgnoreOtherFrames((ChannelFrameType type, ReadOnlySequence<byte> data)[] frames)
        {
            var methodHandler = Substitute.For<Action<DeliverMethod, IMessageProperties, ReadOnlySequence<byte>>>();
            var frameHandler = this.Create(methodHandler);

            foreach (var frame in frames)
            {
                ((IFrameHandler) frameHandler).Handle(frame.type, frame.data);
            }

            Assert.Equal(ContentFrameHandlerState.None, frameHandler.ResetState());
        }

        [Theory]
        [MemberData(nameof(InvalidFramesTestCases))]
        public void ThrowsOnUnexpectedFrames((ChannelFrameType type, ReadOnlySequence<byte> data)[] frames)
        {
            var methodHandler = Substitute.For<Action<DeliverMethod, IMessageProperties, ReadOnlySequence<byte>>>();
            var frameHandler = this.Create(methodHandler);

            Assert.Throws<ConnectionException>(() =>
            {
                foreach (var frame in frames)
                {
                    ((IFrameHandler) frameHandler).Handle(frame.type, frame.data);
                }
            });
        }

        public static IEnumerable<object[]> ValidFramesTestCases()
        {
            var deliverMethod = new DeliverMethod("exchange", "routingKey", "consumer", 42, true);
            var deliverMethodBytes = new byte[]
            {
                0x00,0x3C,0x00,0x3C,  0x08, 0x63, 0x6F, 0x6E, 0x73, 0x75, 0x6D, 0x65, 0x72, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2A, 0x01, 0x08, 0x65, 0x78, 0x63, 0x68, 0x61, 0x6E, 0x67, 0x65, 0x0A, 0x72, 0x6F, 0x75, 0x74, 0x69, 0x6E, 0x67, 0x4B, 0x65, 0x79
            };

            var properties = new MessageProperties {MessageId = "messageId"};
            var propertiesBytes = new byte[]
            {
                0b_00000000, 0b_10000000, 0x09, 0x6D, 0x65, 0x73, 0x73, 0x61, 0x67, 0x65, 0x49, 0x64
            };
            var contentBody = new byte[]
            {
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15
            };

            yield return new object[]
            {
                deliverMethod, properties, contentBody,
                new[]
                {
                    (ChannelFrameType.Method, Helpers.MakeSequence(deliverMethodBytes)),
                    (ChannelFrameType.ContentHeader, Helpers.MakeSequence(propertiesBytes)),
                    (ChannelFrameType.ContentBody, Helpers.MakeSequence(contentBody)),
                }
            };

            yield return new object[]
            {
                deliverMethod, properties, contentBody,
                new[]
                {
                    (ChannelFrameType.Method, Helpers.MakeSequence(deliverMethodBytes, 5)),
                    (ChannelFrameType.ContentHeader, Helpers.MakeSequence(propertiesBytes, 5)),
                    (ChannelFrameType.ContentBody, Helpers.MakeSequence(contentBody, 5)),
                }
            };

            yield return new object[]
            {
                deliverMethod, properties, contentBody,
                new[]
                {
                    (ChannelFrameType.Method, Helpers.MakeSequence(deliverMethodBytes, 2, 5)),
                    (ChannelFrameType.ContentHeader, Helpers.MakeSequence(propertiesBytes, 2 ,5)),
                    (ChannelFrameType.ContentBody, Helpers.MakeSequence(contentBody, 2, 5)),
                }
            };
        }

        public static IEnumerable<object[]> IgnoreFramesTestCases()
        {
            yield return new object[]
            {
                new[]
                {
                    (ChannelFrameType.Method, Helpers.MakeSequence(new byte[] { 0x00, 0x3C, 0x00, 0x32 }))
                }
            };

            yield return new object[]
            {
                new[]
                {
                    (ChannelFrameType.ContentHeader, Helpers.MakeSequence(new byte[] { 0x00, 0x00 })),
                }
            };

            yield return new object[]
            {
                new[]
                {
                    (ChannelFrameType.ContentBody, Helpers.MakeSequence(new byte[] { 0x00, 0x00 }))
                }
            };

            yield return new object[]
            {
                new[]
                {
                    (ChannelFrameType.Method, Helpers.MakeSequence(new byte[] {  0x00, 0x3C, 0x00, 0x32 })),
                    (ChannelFrameType.ContentHeader, Helpers.MakeSequence(new byte[] { 0x00, 0x00 }))
                }
            };

            yield return new object[]
            {
                new[]
                {
                    (ChannelFrameType.Method, Helpers.MakeSequence(new byte[] { 0x00, 0x3C, 0x00, 0x32 })),
                    (ChannelFrameType.ContentHeader, Helpers.MakeSequence(new byte[] { 0x00, 0x00 })),
                    (ChannelFrameType.ContentBody, Helpers.MakeSequence(new byte[] { 0x00, 0x00 }))
                }
            };
        }

        public static IEnumerable<object[]> InvalidFramesTestCases()
        {
            yield return new object[]
            {
                new[]
                {
                    (ChannelFrameType.Unknown, Helpers.MakeSequence(new byte[] { 0x00, 0x00 }))
                }
            };

            var deliverBytes = new byte[]
            {
                0x00,0x3C,0x00,0x3C,  0x08, 0x63, 0x6F, 0x6E, 0x73, 0x75, 0x6D, 0x65, 0x72, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2A, 0x01, 0x08, 0x65, 0x78, 0x63, 0x68, 0x61, 0x6E, 0x67, 0x65, 0x0A, 0x72, 0x6F, 0x75, 0x74, 0x69, 0x6E, 0x67, 0x4B, 0x65, 0x79
            };
            var propertiesBytes = new byte[]
            {
                0b_00000000, 0b_10000000, 0x09, 0x6D, 0x65, 0x73, 0x73, 0x61, 0x67, 0x65, 0x49, 0x64
            };

            yield return new object[]
            {
                new[]
                {
                    (ChannelFrameType.Method, Helpers.MakeSequence(deliverBytes)),
                    (ChannelFrameType.Method, Helpers.MakeSequence(new byte[] { 0x00, 0x3C, 0x00, 0x32 })),
                }
            };

            yield return new object[]
            {
                new[]
                {
                    (ChannelFrameType.Method, Helpers.MakeSequence(deliverBytes)),
                    (ChannelFrameType.Method, Helpers.MakeSequence(deliverBytes)),
                }
            };

            yield return new object[]
            {
                new[]
                {
                    (ChannelFrameType.Method, Helpers.MakeSequence(deliverBytes)),
                    (ChannelFrameType.ContentBody, Helpers.MakeSequence(new byte[] { 0x00, 0x3C, 0x00, 0x32 })),
                }
            };

            yield return new object[]
            {
                new[]
                {
                    (ChannelFrameType.Method, Helpers.MakeSequence(deliverBytes)),
                    (ChannelFrameType.ContentHeader, Helpers.MakeSequence(propertiesBytes)),
                    (ChannelFrameType.Method, Helpers.MakeSequence(deliverBytes)),
                }
            };

            yield return new object[]
            {
                new[]
                {
                    (ChannelFrameType.Method, Helpers.MakeSequence(deliverBytes)),
                    (ChannelFrameType.ContentHeader, Helpers.MakeSequence(propertiesBytes)),
                    (ChannelFrameType.ContentHeader, Helpers.MakeSequence(propertiesBytes)),
                }
            };
        }

        private ContentFrameHandler<DeliverMethod> Create(Action<DeliverMethod, IMessageProperties, ReadOnlySequence<byte>> handler)
            => new ContentFrameHandler<DeliverMethod>((uint) MethodId.BasicDeliver, new DeliverMethodParser(), handler, new BufferPool(10_000));
    }
}