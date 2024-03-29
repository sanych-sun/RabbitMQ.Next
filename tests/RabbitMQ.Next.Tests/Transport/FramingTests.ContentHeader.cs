using System;
using System.Collections.Generic;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.Transport;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport;

public partial class FramingTests
{
    [Theory]
    [MemberData(nameof(MessagePropertiesTestCases))]
    internal void WriteContentProperties(byte[] expected, MessageProperties props)
    {
        Span<byte> buffer = new byte[1024];
        var res = buffer.WriteContentProperties(props);
        var written = buffer.Length - res.Length;
        var bytes = buffer.Slice(0, written);
        
        Assert.Equal(expected.Length, written);
        Assert.Equal(expected, bytes.ToArray());
    }

    public static IEnumerable<object[]> MessagePropertiesTestCases()
    {
        yield return new object[]
        {
            new byte[] {  0b_00000000, 0b_00000000 },
            new MessageProperties(),
        };

        yield return new object[]
        {
            new byte[] { 0b_10000000, 0b_00000000, 0x04, 0x6A, 0x73, 0x6F, 0x6E },
            new MessageProperties { ContentType = "json" },
        };

        yield return new object[]
        {
            new byte[] { 0b_01000000, 0b_00000000, 0x04, 0x75, 0x74, 0x66, 0x38 },
            new MessageProperties { ContentEncoding = "utf8" },
        };

        yield return new object[]
        {
            new byte[] { 0b_00100000, 0b_00000000, 0x00, 0x00, 0x00, 0x0E, 0x03, 0x6B, 0x65, 0x79, 0x53, 0x00, 0x00, 0x00, 0x05, 0x76, 0x61, 0x6C, 0x75, 0x65 },
            new MessageProperties { Headers = new Dictionary<string, object> { ["key"] = "value" } },
        };

        yield return new object[]
        {
            new byte[] { 0b_00010000, 0b_00000000, 0x02 },
            new MessageProperties { DeliveryMode = DeliveryMode.Persistent },
        };

        yield return new object[]
        {
            new byte[] { 0b_00001000, 0b_00000000, 0x05 },
            new MessageProperties { Priority = 5 },
        };

        yield return new object[]
        {
            new byte[] { 0b_00000100, 0b_00000000, 0x0D, 0x63, 0x6F, 0x72, 0x72, 0x65, 0x6C, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x49, 0x64 },
            new MessageProperties { CorrelationId = "correlationId" },
        };

        yield return new object[]
        {
            new byte[] { 0b_00000010, 0b_00000000, 0x07, 0x72, 0x65, 0x70, 0x6C, 0x79, 0x54, 0x6F },
            new MessageProperties { ReplyTo = "replyTo" },
        };

        yield return new object[]
        {
            new byte[] { 0b_00000001, 0b_00000000, 0x0A, 0x65, 0x78, 0x70, 0x69, 0x72, 0x61, 0x74, 0x69, 0x6F, 0x6E },
            new MessageProperties { Expiration = "expiration" },
        };

        yield return new object[]
        {
            new byte[] { 0b_00000000, 0b_10000000, 0x09, 0x6D, 0x65, 0x73, 0x73, 0x61, 0x67, 0x65, 0x49, 0x64 },
            new MessageProperties { MessageId = "messageId" },
        };

        yield return new object[]
        {
            new byte[] { 0b_00000000, 0b_01000000, 0x00, 0x00, 0x00, 0x00, 0x19, 0xD7, 0x87, 0x00 },
            new MessageProperties { Timestamp = new DateTimeOffset(1983, 09, 28, 0, 0, 0, TimeSpan.Zero) },
        };

        yield return new object[]
        {
            new byte[] { 0b_00000000, 0b_00100000, 0x04, 0x74, 0x79, 0x70, 0x65 },
            new MessageProperties { Type = "type" },
        };

        yield return new object[]
        {
            new byte[] { 0b_00000000, 0b_00010000, 0x06, 0x75, 0x73, 0x65, 0x72, 0x49, 0x64 },
            new MessageProperties { UserId = "userId" },
        };

        yield return new object[]
        {
            new byte[] { 0b_00000000, 0b_00001000, 0x05, 0x61, 0x70, 0x70, 0x49, 0x64 },
            new MessageProperties { ApplicationId = "appId" },
        };

        yield return new object[]
        {
            new byte[]
            {
                0b_11111111, 0b_11111000,
                0x04, 0x6A, 0x73, 0x6F, 0x6E,
                0x04, 0x75, 0x74, 0x66, 0x38,
                0x00, 0x00, 0x00, 0x0E, 0x03, 0x6B, 0x65, 0x79, 0x53, 0x00, 0x00, 0x00, 0x05, 0x76, 0x61, 0x6C, 0x75, 0x65,
                0x02,
                0x05,
                0x0D, 0x63, 0x6F, 0x72, 0x72, 0x65, 0x6C, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x49, 0x64,
                0x07, 0x72, 0x65, 0x70, 0x6C, 0x79, 0x54, 0x6F,
                0x0A, 0x65, 0x78, 0x70, 0x69, 0x72, 0x61, 0x74, 0x69, 0x6F, 0x6E,
                0x09, 0x6D, 0x65, 0x73, 0x73, 0x61, 0x67, 0x65, 0x49, 0x64,
                0x00, 0x00, 0x00, 0x00, 0x19, 0xD7, 0x87, 0x00,
                0x04, 0x74, 0x79, 0x70, 0x65,
                0x06, 0x75, 0x73, 0x65, 0x72, 0x49, 0x64,
                0x05, 0x61, 0x70, 0x70, 0x49, 0x64,
            },
            new MessageProperties
            {
                ContentType = "json",
                ContentEncoding = "utf8",
                DeliveryMode = DeliveryMode.Persistent,
                Priority = 5,
                Headers = new Dictionary<string, object> { ["key"] = "value" },
                CorrelationId = "correlationId",
                ReplyTo = "replyTo",
                Expiration = "expiration",
                MessageId = "messageId",
                Timestamp = new DateTimeOffset(1983, 09, 28, 0, 0, 0, TimeSpan.Zero),
                Type = "type",
                UserId = "userId",
                ApplicationId = "appId",
            },
        };
    }
}
