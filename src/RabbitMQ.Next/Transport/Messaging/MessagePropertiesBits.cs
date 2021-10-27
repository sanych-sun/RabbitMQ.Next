namespace RabbitMQ.Next.Transport.Messaging
{
    internal static class MessagePropertiesBits
    {
        public static readonly byte ContentType = 15;
        public static readonly byte ContentEncoding = 14;
        public static readonly byte Headers = 13;
        public static readonly byte DeliveryMode = 12;
        public static readonly byte Priority = 11;
        public static readonly byte CorrelationId = 10;
        public static readonly byte ReplyTo = 9;
        public static readonly byte Expiration = 8;
        public static readonly byte MessageId = 7;
        public static readonly byte Timestamp = 6;
        public static readonly byte Type = 5;
        public static readonly byte UserId = 4;
        public static readonly byte ApplicationId = 3;
    }
}