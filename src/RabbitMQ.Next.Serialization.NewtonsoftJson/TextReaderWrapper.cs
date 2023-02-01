using System.Buffers;
using System.IO;
using System.Text;

namespace RabbitMQ.Next.Serialization.NewtonsoftJson;

internal sealed class TextReaderWrapper: TextReader
{
    private readonly Decoder decoder;
    private readonly char[] decoded;
    private ReadOnlySequence<byte> reader;
    private int decodedIndex = -1;
    private int decodedLen;
    private bool isDisposed;

    public TextReaderWrapper(Encoding encoding, ReadOnlySequence<byte> reader)
    {
        this.decoder = encoding.GetDecoder();
        this.reader = reader;
        this.decoded = ArrayPool<char>.Shared.Rent(1000);
    }

    protected override void Dispose(bool disposing)
    {
        if (!this.isDisposed)
        {
            this.isDisposed = true;
            ArrayPool<char>.Shared.Return(this.decoded);
        }

        base.Dispose(disposing);
    }

    public override int Peek()
    {
        if (this.EnsureDecodedBuffer())
        {
            return this.decoded[this.decodedIndex + 1];
        }

        return -1;
    }

    public override int Read()
    {
        if (this.EnsureDecodedBuffer())
        {
            this.decodedIndex++;
            return this.decoded[this.decodedIndex];
        }

        return -1;
    }

    private bool EnsureDecodedBuffer()
    {
        if (this.decodedIndex + 1 >= this.decodedLen)
        {
            this.decodedIndex = -1;
            
            while (!this.reader.IsEmpty)
            {
                this.decoder.Convert(this.reader.FirstSpan, this.decoded, false, out var bytesUsed, out this.decodedLen, out var completed);
                this.reader = this.reader.Slice(bytesUsed);

                if (this.decodedLen > 0)
                {
                    break;
                }
            }
        }

        return this.decodedLen > 0;
    }
}