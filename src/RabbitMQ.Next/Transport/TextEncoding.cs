using System;
using System.Text;

namespace RabbitMQ.Next.Transport;

public static class TextEncoding
{
    private static readonly Encoding Encoding = Encoding.UTF8;

    public static int GetBytes(string text, Span<byte> destination) => Encoding.GetBytes(text, destination);
    
    public static int GetMaxBytes(int len) => Encoding.GetMaxByteCount(len);

    public static string GetString(ReadOnlySpan<byte> bytes) => Encoding.GetString(bytes);
}