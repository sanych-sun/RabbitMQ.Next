using System;
using System.Buffers;
using System.Collections.Generic;
using RabbitMQ.Next.Serialization.Formatters;
using RabbitMQ.Next.Transport.Buffers;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.Formatters
{
    public class StringFormatterTests
    {
        [Theory]
        [MemberData(nameof(FormatTestCases))]
        public void CanFormat(string data, int initialBufferSize, byte[] expected)
        {
            var formatter = new StringFormatter();
            var bufferWriter = new ArrayBufferWriter<byte>(initialBufferSize);

            formatter.Format(data, bufferWriter);

            Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
        }

        [Theory]
        [MemberData(nameof(ParseTestCases))]
        public void CanParse(string expected, byte[][] contentparts)
        {
            var formatter = new StringFormatter();
            var sequence = Helpers.MakeSequence(contentparts);

            var result = formatter.Parse(sequence);

            Assert.Equal(expected, result);
        }

        public static IEnumerable<object[]> FormatTestCases()
        {
            yield return new object[] { string.Empty, 50, Array.Empty<byte>()};

            var texts = Helpers.GetDummyTexts(0, 128);

            foreach (var text in texts)
            {
                yield return new object[] { text.Text, 50, text.Bytes.ToArray() };
            }
        }

        public static IEnumerable<object[]> ParseTestCases()
        {
            IEnumerable<byte[]> Slice(ReadOnlyMemory<byte> data, int chunkSize)
            {
                while (data.Length > 0)
                {
                    if (data.Length <= chunkSize)
                    {
                        yield return data.ToArray();
                        yield break;
                    }

                    yield return data.Slice(0, chunkSize).ToArray();
                    data = data.Slice(chunkSize);
                }
            }

            yield return new object[] { string.Empty, new [] { new byte[0] }};

            var texts = Helpers.GetDummyTexts(0, 128);

            foreach (var text in texts)
            {
                yield return new object[] { text.Text, new[] { text.Bytes.ToArray() }};
                if (text.Bytes.Length > 50)
                {
                    yield return new object[] { text.Text, Slice(text.Bytes, 50) };
                }
            }
        }
    }
}