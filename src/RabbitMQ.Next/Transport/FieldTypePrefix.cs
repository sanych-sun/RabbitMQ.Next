namespace RabbitMQ.Next.Transport;

internal static class FieldTypePrefix
{
    // List of field type prefixes does not conform to AMQP 0-9-1 spec!
    // it was created based on official .net and java library
    // long-long-int (L) - does not supported
    // short-string (s) - does not supported
    // short-uint (u) - does not supported
    // short-int uses (byte)'s' prefix instead of (byte)'U'
    // binary (x) - non-standard data type for raw bytes

    public const byte Boolean = (byte) 't';

    public const byte SByte = (byte)'b';

    public const byte Byte = (byte)'B';

    public const byte Long = (byte)'l';

    public const byte Int = (byte)'I';

    public const byte Short = (byte)'s';

    public const byte UInt = (byte)'i';

    public const byte Array = (byte)'A';

    public const byte Timestamp = (byte)'T';

    public const byte Null = (byte)'V';

    public const byte String = (byte)'S';

    public const byte Table = (byte)'F';

    public const byte Single = (byte)'f';

    public const byte Decimal = (byte)'D';

    public const byte Double = (byte)'d';

    public const byte Binary = (byte)'x';
}