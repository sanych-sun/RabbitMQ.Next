using System;
using System.Collections.Generic;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.Transport.Messaging;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Messaging
{
    public class ContentHeaderTests
    {
        [Theory]
        [MemberData(nameof(WriteContentHeaderTestCases))]
        internal void WriteContentHeader(byte[] expected, MessageProperties props, ulong size)
        {
            var buffer = new byte[expected.Length];
            var result = ((Memory<byte>)buffer).WriteContentHeader(props, size);

            Assert.Equal(expected, buffer);
            Assert.Equal(expected.Length, result);
        }

        public static IEnumerable<object[]> WriteContentHeaderTestCases()
        {
            yield return new object[]
            {
                new byte[] { 0, 60, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0b_00000000, 0b_00000000 },
                null, 1
            };

            yield return new object[]
            {
                new byte[] { 0, 60, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0b_00000000, 0b_00000000 },
                new MessageProperties(), 1
            };

            yield return new object[]
            {
                new byte[] { 0, 60, 0, 0, 0, 0, 0, 0, 0, 0, 0, 42, 0b_10000000, 0b_00000000, 0x04, 0x6A, 0x73, 0x6F, 0x6E},
                new MessageProperties { ContentType = "json"}, 42
            };
        }
    }
}