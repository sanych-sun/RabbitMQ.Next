using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Transport.Messaging
{
    internal sealed class LazyMessageProperties : IMessageProperties
    {
        private MessageFlags flags;
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
            buffer.Span.Read(out ushort fl);
            this.flags = (MessageFlags)fl;

            buffer[sizeof(ushort)..]
                .SplitStringProperty(out this.contentType, this.flags, MessageFlags.ContentType)
                .SplitStringProperty(out this.contentEncoding, this.flags, MessageFlags.ContentEncoding)
                .SplitDynamicProperty(out this.headers, this.flags, MessageFlags.Headers)
                .SplitFixedProperty(out this.deliveryMode, this.flags, MessageFlags.DeliveryMode, sizeof(byte))
                .SplitFixedProperty(out this.priority, this.flags, MessageFlags.Priority, sizeof(byte))
                .SplitStringProperty(out this.correlationId, this.flags, MessageFlags.CorrelationId)
                .SplitStringProperty(out this.replyTo, this.flags, MessageFlags.ReplyTo)
                .SplitStringProperty(out this.expiration, this.flags, MessageFlags.Expiration)
                .SplitStringProperty(out this.messageId, this.flags, MessageFlags.MessageId)
                .SplitFixedProperty(out this.timestamp, this.flags, MessageFlags.Timestamp, sizeof(ulong))
                .SplitStringProperty(out this.type, this.flags, MessageFlags.Type)
                .SplitStringProperty(out this.userId, this.flags, MessageFlags.UserId)
                .SplitStringProperty(out this.applicationId, this.flags, MessageFlags.ApplicationId);
        }

        public void Reset()
        {
            this.flags = MessageFlags.None;
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

        public MessageFlags Flags => this.flags;

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