using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Transport.Messaging
{
    internal sealed class LazyMessageProperties : IMessageProperties
    {
        private ReadOnlyMemory<byte> contentType;
        private ReadOnlyMemory<byte> contentEncoding;
        private ReadOnlyMemory<byte> headers;
        private ReadOnlyMemory<byte> deliveryMode;
        private ReadOnlyMemory<byte> priority;
        private ReadOnlyMemory<byte> correlationId;
        private ReadOnlyMemory<byte> replyTo;
        private ReadOnlyMemory<byte> expiration;
        private ReadOnlyMemory<byte> messageId;
        private ReadOnlyMemory<byte> timestamp;
        private ReadOnlyMemory<byte> type;
        private ReadOnlyMemory<byte> userId;
        private ReadOnlyMemory<byte> applicationId;

        public void Set(ReadOnlyMemory<byte> buffer)
        {
            buffer.Span.Read(out ushort flags);
            this.Flags = (MessageFlags)flags;

            buffer[sizeof(ushort)..]
                .SplitStringProperty(out this.contentType, flags, 15)
                .SplitStringProperty(out this.contentEncoding, flags, 14)
                .SplitDynamicProperty(out this.headers, flags, 13)
                .SplitFixedProperty(out this.deliveryMode, flags, 12, sizeof(byte))
                .SplitFixedProperty(out this.priority, flags, 11, sizeof(byte))
                .SplitStringProperty(out this.correlationId, flags, 10)
                .SplitStringProperty(out this.replyTo, flags, 9)
                .SplitStringProperty(out this.expiration, flags, 8)
                .SplitStringProperty(out this.messageId, flags, 7)
                .SplitFixedProperty(out this.timestamp, flags, 6, sizeof(ulong))
                .SplitStringProperty(out this.type, flags, 5)
                .SplitStringProperty(out this.userId, flags, 4)
                .SplitStringProperty(out this.applicationId, flags, 3);
        }

        public void Reset()
        {
            this.Flags = MessageFlags.None;
            this.contentType = ReadOnlyMemory<byte>.Empty;
            this.contentEncoding = ReadOnlyMemory<byte>.Empty;
            this.headers = ReadOnlyMemory<byte>.Empty;
            this.deliveryMode = ReadOnlyMemory<byte>.Empty;
            this.priority = ReadOnlyMemory<byte>.Empty;
            this.correlationId = ReadOnlyMemory<byte>.Empty;
            this.replyTo = ReadOnlyMemory<byte>.Empty;
            this.expiration = ReadOnlyMemory<byte>.Empty;
            this.messageId = ReadOnlyMemory<byte>.Empty;
            this.timestamp = ReadOnlyMemory<byte>.Empty;
            this.type = ReadOnlyMemory<byte>.Empty;
            this.userId = ReadOnlyMemory<byte>.Empty;
            this.applicationId = ReadOnlyMemory<byte>.Empty;
        }

        public MessageFlags Flags { get; private set; }

        public string ContentType => this.DecodeString(this.contentType);

        public string ContentEncoding => this.DecodeString(this.contentEncoding);

        public IReadOnlyDictionary<string, object> Headers => this.DecodeTable(this.headers);

        public DeliveryMode DeliveryMode => (DeliveryMode)this.DecodeByte(this.deliveryMode);

        public byte Priority => this.DecodeByte(this.priority);

        public string CorrelationId => this.DecodeString(this.correlationId);

        public string ReplyTo => this.DecodeString(this.replyTo);

        public string Expiration => this.DecodeString(this.expiration);

        public string MessageId => this.DecodeString(this.messageId);

        public DateTimeOffset Timestamp => this.DecodeTimestamp(this.timestamp);

        public string Type => this.DecodeString(this.type);

        public string UserId => this.DecodeString(this.userId);

        public string ApplicationId => this.DecodeString(this.applicationId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string DecodeString(ReadOnlyMemory<byte> data)
        {
            if (data.IsEmpty)
            {
                return null;
            }

            return TextEncoding.GetString(data.Span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DateTimeOffset DecodeTimestamp(ReadOnlyMemory<byte> data)
        {
            if (data.IsEmpty)
            {
                return default;
            }

            data.Span.Read(out DateTimeOffset val);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IReadOnlyDictionary<string, object> DecodeTable(ReadOnlyMemory<byte> data)
        {
            if (data.IsEmpty)
            {
                return null;
            }

            data.Span.Read(out Dictionary<string, object> val);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte DecodeByte(ReadOnlyMemory<byte> data)
        {
            if (data.IsEmpty)
            {
                return default;
            }

            data.Span.Read(out byte val);
            return val;
        }
    }
}