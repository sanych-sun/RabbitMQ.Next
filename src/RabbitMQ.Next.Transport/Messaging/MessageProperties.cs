using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Transport.Messaging
{
    internal sealed class MessageProperties : IMessageProperties, IDisposable
    {
        private readonly StringProperty contentType;
        private readonly StringProperty contentEncoding;
        private readonly TableProperty headers;
        private readonly byte? deliveryMode;
        private readonly byte? priority;
        private readonly StringProperty correlationId;
        private readonly StringProperty replyTo;
        private readonly StringProperty expiration;
        private readonly StringProperty messageId;
        private readonly DateTimeOffset? timestamp;
        private readonly StringProperty type;
        private readonly StringProperty userId;
        private readonly StringProperty applicationId;

        private bool isDisposed;

        public MessageProperties(ReadOnlySequence<byte> buffer)
        {
            // TODO: do some magic here in case multi segments.
            var data = buffer.First;

            data.Span.Read(out ushort flags);

            data.Slice(sizeof(ushort))
                .ReadProperty(out this.contentType, flags, 15)
                .ReadProperty(out this.contentEncoding, flags, 14)
                .ReadProperty(out this.headers, flags, 13)
                .ReadProperty(out this.deliveryMode, flags, 12)
                .ReadProperty(out this.priority, flags, 11)
                .ReadProperty(out this.correlationId, flags, 10)
                .ReadProperty(out this.replyTo, flags, 9)
                .ReadProperty(out this.expiration, flags, 8)
                .ReadProperty(out this.messageId, flags, 7)
                .ReadProperty(out this.timestamp, flags, 6)
                .ReadProperty(out this.type, flags, 5)
                .ReadProperty(out this.userId, flags, 4)
                .ReadProperty(out this.applicationId, flags, 3);
        }


        public string ContentType
        {
            get
            {
                this.CheckDisposed();
                return this.contentType.Value;
            }
        }

        public string ContentEncoding
        {
            get
            {
                this.CheckDisposed();
                return this.contentEncoding.Value;
            }
        }

        public IReadOnlyDictionary<string, object> Headers
        {
            get
            {
                this.CheckDisposed();
                return this.headers.Value;
            }
        }

        public DeliveryMode DeliveryMode
        {
            get
            {
                this.CheckDisposed();
                if (this.deliveryMode.HasValue)
                {
                    return (DeliveryMode) this.deliveryMode.Value;
                }

                return DeliveryMode.Unset;
            }
        }

        public byte? Priority
        {
            get
            {
                this.CheckDisposed();
                return this.priority;
            }
        }

        public string CorrelationId
        {
            get
            {
                this.CheckDisposed();
                return this.correlationId.Value;
            }
        }

        public string ReplyTo
        {
            get
            {
                this.CheckDisposed();
                return this.replyTo.Value;
            }
        }

        public string Expiration
        {
            get
            {
                this.CheckDisposed();
                return this.expiration.Value;
            }
        }

        public string MessageId
        {
            get
            {
                this.CheckDisposed();
                return this.messageId.Value;
            }
        }

        public DateTimeOffset? Timestamp
        {
            get
            {
                this.CheckDisposed();
                return this.timestamp;
            }
        }

        public string Type
        {
            get
            {
                this.CheckDisposed();
                return this.type.Value;
            }
        }

        public string UserId
        {
            get
            {
                this.CheckDisposed();
                return this.userId.Value;
            }
        }

        public string ApplicationId
        {
            get
            {
                this.CheckDisposed();
                return this.applicationId.Value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(MessageProperties));
            }
        }

        public void Dispose()
        {
            this.isDisposed = true;
        }
    }
}