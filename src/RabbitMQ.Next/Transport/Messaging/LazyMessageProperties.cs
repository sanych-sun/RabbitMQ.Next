using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Transport.Messaging
{
    internal sealed class LazyMessageProperties : IMessageProperties, IDisposable
    {
        private readonly ReadOnlyMemory<byte> contentType;
        private readonly ReadOnlyMemory<byte> contentEncoding;
        private readonly ReadOnlyMemory<byte> headers;
        private readonly ReadOnlyMemory<byte> deliveryMode;
        private readonly ReadOnlyMemory<byte> priority;
        private readonly ReadOnlyMemory<byte> correlationId;
        private readonly ReadOnlyMemory<byte> replyTo;
        private readonly ReadOnlyMemory<byte> expiration;
        private readonly ReadOnlyMemory<byte> messageId;
        private readonly ReadOnlyMemory<byte> timestamp;
        private readonly ReadOnlyMemory<byte> type;
        private readonly ReadOnlyMemory<byte> userId;
        private readonly ReadOnlyMemory<byte> applicationId;

        private bool isDisposed;

        public LazyMessageProperties(ReadOnlyMemory<byte> buffer)
        {
            buffer.Read(out ushort flags)
                .SplitStringProperty(out this.contentType, flags, 15)
                .SplitStringProperty(out this.contentEncoding, flags, 14)
                .SplitTableProperty(out this.headers, flags, 13)
                .SplitFixedSizeProperty(out this.deliveryMode, flags, 12, sizeof(byte))
                .SplitFixedSizeProperty(out this.priority, flags, 11, sizeof(byte))
                .SplitStringProperty(out this.correlationId, flags, 10)
                .SplitStringProperty(out this.replyTo, flags, 9)
                .SplitStringProperty(out this.expiration, flags, 8)
                .SplitStringProperty(out this.messageId, flags, 7)
                .SplitFixedSizeProperty(out this.timestamp, flags, 6, sizeof(ulong))
                .SplitStringProperty(out this.type, flags, 5)
                .SplitStringProperty(out this.userId, flags, 4)
                .SplitStringProperty(out this.applicationId, flags, 3);
        }

        public string ContentType
        {
            get
            {
                this.CheckDisposed();
                return this.DecodeString(this.contentType);
            }
        }

        public string ContentEncoding
        {
            get
            {
                this.CheckDisposed();
                return this.DecodeString(this.contentEncoding);
            }
        }

        public IReadOnlyDictionary<string, object> Headers
        {
            get
            {
                this.CheckDisposed();
                return this.DecodeTable(this.headers);
            }
        }

        public DeliveryMode DeliveryMode
        {
            get
            {
                this.CheckDisposed();
                var vl = this.DecodeByte(this.deliveryMode);
                if (vl.HasValue)
                {
                    return (DeliveryMode)vl;
                }

                return DeliveryMode.Unset;
            }
        }

        public byte? Priority
        {
            get
            {
                this.CheckDisposed();
                return this.DecodeByte(this.priority);
            }
        }

        public string CorrelationId
        {
            get
            {
                this.CheckDisposed();
                return this.DecodeString(this.correlationId);
            }
        }

        public string ReplyTo
        {
            get
            {
                this.CheckDisposed();
                return this.DecodeString(this.replyTo);
            }
        }

        public string Expiration
        {
            get
            {
                this.CheckDisposed();
                return this.DecodeString(this.expiration);
            }
        }

        public string MessageId
        {
            get
            {
                this.CheckDisposed();
                return this.DecodeString(this.messageId);
            }
        }

        public DateTimeOffset? Timestamp
        {
            get
            {
                this.CheckDisposed();
                return this.DecodeTimestamp(this.timestamp);
            }
        }

        public string Type
        {
            get
            {
                this.CheckDisposed();
                return this.DecodeString(this.type);
            }
        }

        public string UserId
        {
            get
            {
                this.CheckDisposed();
                return this.DecodeString(this.userId);
            }
        }

        public string ApplicationId
        {
            get
            {
                this.CheckDisposed();
                return this.DecodeString(this.applicationId);
            }
        }

        public void Dispose()
        {
            this.isDisposed = true;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(LazyMessageProperties));
            }
        }

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
        private DateTimeOffset? DecodeTimestamp(ReadOnlyMemory<byte> data)
        {
            if (data.IsEmpty)
            {
                return null;
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
        private byte? DecodeByte(ReadOnlyMemory<byte> data)
        {
            if (data.IsEmpty)
            {
                return null;
            }

            data.Span.Read(out byte val);
            return val;
        }
    }
}