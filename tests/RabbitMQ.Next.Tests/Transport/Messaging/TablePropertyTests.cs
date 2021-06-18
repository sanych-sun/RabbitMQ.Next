using System;
using System.Collections.Generic;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.Transport.Messaging;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Messaging
{
    public class TablePropertyTests
    {
        [Theory]
        [MemberData(nameof(ReadTestCases))]
        public void CanParse(byte[] data, IReadOnlyDictionary<string, object> expected)
        {
            var prop = new TableProperty(data);

            Assert.True(Helpers.DictionaryEquals(expected, prop.Value));
        }

        [Fact]
        public void ShouldNotParseTwice()
        {
            var prop = new TableProperty(new byte[] {0, 0, 0, 14, 3, 107, 101, 121, 83, 0, 0, 0, 5, 118, 97, 108, 117, 101});

            Assert.Same(prop.Value, prop.Value);
        }

        public static IEnumerable<object[]> ReadTestCases()
        {
            yield return new object[] { new byte[0], null };
            yield return new object[] { new byte[] { 0, 0, 0, 0 }, null };
            yield return new object[] { new byte[] { 0, 0, 0, 14, 3, 107, 101, 121, 83, 0, 0, 0, 5, 118, 97, 108, 117, 101 }, new Dictionary<string, object> { ["key"] = "value" }};
        }
    }
}