namespace RabbitMQ.Next.Transport.Messaging
{
    internal static class MessagePropertiesBits
    {
        public const byte ContentType = 15;
        public const byte ContentEncoding = 14;
        public const byte Headers = 13;
        public const byte DeliveryMode = 12;
        public const byte Priority = 11;
        public const byte CorrelationId = 10;
        public const byte ReplyTo = 9;
        public const byte Expiration = 8;
        public const byte MessageId = 7;
        public const byte Timestamp = 6;
        public const byte Type = 5;
        public const byte UserId = 4;
        public const byte ApplicationId = 3;
    }
}