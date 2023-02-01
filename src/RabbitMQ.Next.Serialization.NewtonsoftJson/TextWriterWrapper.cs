using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace RabbitMQ.Next.Serialization.NewtonsoftJson;

internal sealed class TextWriterWrapper: TextWriter
{
    private readonly IBufferWriter<byte> writer;
    private readonly int charByteMaxSize;

    public TextWriterWrapper(Encoding encoding, IBufferWriter<byte> writer)
    {
        this.writer = writer;
        this.Encoding = encoding;
        this.charByteMaxSize = encoding.GetMaxByteCount(1);
    }

    public override Encoding Encoding { get; }

    public override void Write(char value)
    {
        Span<char> chars = stackalloc char[1];
        chars[0] = value;
        var buffer = this.writer.GetSpan(this.charByteMaxSize);
        var written = this.Encoding.GetBytes(chars, buffer);
        this.writer.Advance(written);
    }
}