using System;
using System.Text;

namespace RabbitMQ.Next.Transport
{
    public static class TextEncoding
    {
        private static readonly Encoding Encoding = Encoding.UTF8;

        public static int GetMaxByteCount(int chars) => Encoding.GetMaxByteCount(chars);

        public static int GetMaxCharCount(int bytes) => Encoding.GetMaxCharCount(bytes);

        public static int GetBytes(string text, Span<byte> destination) => Encoding.GetBytes(text, destination);

        public static string GetString(ReadOnlySpan<byte> bytes) => Encoding.GetString(bytes);

        public static Encoder GetEncoder() => Encoding.GetEncoder();

        public static Decoder GetDecoder() => Encoding.GetDecoder();
    }
}