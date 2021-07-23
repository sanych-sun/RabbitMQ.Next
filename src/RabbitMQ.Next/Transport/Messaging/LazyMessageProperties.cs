using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Transport.Messaging
{
    internal sealed class LazyMessageProperties : IMessageProperties, IDisposable
    {
        private readonly ReadOnlySequence<byte> contentType;
        private readonly ReadOnlySequence<byte> contentEncoding;
        private readonly ReadOnlySequence<byte> headers;
        private readonly ReadOnlySequence<byte> deliveryMode;
        private readonly ReadOnlySequence<byte> priority;
        private readonly ReadOnlySequence<byte> correlationId;
        private readonly ReadOnlySequence<byte> replyTo;
        private readonly ReadOnlySequence<byte> expiration;
        private readonly ReadOnlySequence<byte> messageId;
        private readonly ReadOnlySequence<byte> timestamp;
        private readonly ReadOnlySequence<byte> type;
        private readonly ReadOnlySequence<byte> userId;
        private readonly ReadOnlySequence<byte> applicationId;

        private bool isDisposed;

        public LazyMessageProperties(ReadOnlySequence<byte> buffer)
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
        private string DecodeString(ReadOnlySequence<byte> data)
        {
            if (data.IsEmpty)
            {
                return null;
            }

            if (data.IsSingleSegment)
            {
                return TextEncoding.GetString(data.FirstSpan);
            }

            Span<byte> buffer = stackalloc byte[(int)data.Length];
            data.CopyTo(buffer);
            return TextEncoding.GetString(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DateTimeOffset? DecodeTimestamp(ReadOnlySequence<byte> data)
        {
            if (data.IsEmpty)
            {
                return null;
            }

            DateTimeOffset val;
            if (data.IsSingleSegment)
            {
                data.FirstSpan.Read(out val);
            }
            else
            {
                Span<byte> buffer = stackalloc byte[(int)data.Length];
                data.CopyTo(buffer);
                ((ReadOnlySpan<byte>)buffer).Read(out val);
            }

            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IReadOnlyDictionary<string, object> DecodeTable(ReadOnlySequence<byte> data)
        {
            if (data.IsEmpty)
            {
                return null;
            }

            Dictionary<string, object> val;
            if (data.IsSingleSegment)
            {
                data.FirstSpan.Read(out val);
            }
            else
            {
                Span<byte> buffer = stackalloc byte[(int)data.Length];
                data.CopyTo(buffer);
                ((ReadOnlySpan<byte>)buffer).Read(out val);
            }

            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte? DecodeByte(ReadOnlySequence<byte> data)
        {
            if (data.IsEmpty)
            {
                return null;
            }

            data.FirstSpan.Read(out byte val);
            return val;
        }
    }
}