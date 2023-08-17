using System;
using System.Buffers;
using System.Collections.Generic;
using RabbitMQ.Next.Serialization.PlainText.Converters;
using RabbitMQ.Next.Tests.Mocks;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.PlainText.Converters;

public class StringConverterTests
{
    [Theory]
    [MemberData(nameof(FormatTestCases))]
    public void CanFormat(string content, int initialBufferSize, byte[] expected)
    {
        var converter = new StringConverter();
        var bufferWriter = new ArrayBufferWriter<byte>(initialBufferSize);

        converter.Format(content, bufferWriter);

        Assert.Equal(expected, bufferWriter.WrittenMemory.ToArray());
    }

    [Theory]
    [MemberData(nameof(ParseTestCases))]
    public void CanParse(string expected, params byte[][] parts)
    {
        var converter = new StringConverter();
        var sequence = Helpers.MakeSequence(parts);

        var parsed = converter.Parse(sequence);

        Assert.Equal(expected, parsed);
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
        yield return new object[] { string.Empty, new byte[0] };

        var texts = Helpers.GetDummyTexts(0, 128);

        foreach (var text in texts)
        {
            yield return new object[] { text.Text, text.Bytes.ToArray() };
            if (text.Bytes.Length > 50)
            {
                yield return new object[] { text.Text, text.Bytes.Slice(0, 50).ToArray(), text.Bytes.Slice(50).ToArray() };
            }
        }
    }
}