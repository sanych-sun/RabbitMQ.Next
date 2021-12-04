using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Publisher
{
    internal sealed class ContentAccessor : IContentAccessor
    {
        private readonly ISerializerFactory serializerFactory;
        private ReadOnlySequence<byte> payload;
        private IMessageProperties properties;
        private bool isSet;

        public ContentAccessor(ISerializerFactory serializerFactory)
        {
            if (serializerFactory == null)
            {
                throw new ArgumentNullException(nameof(serializerFactory));
            }

            this.serializerFactory = serializerFactory;
        }

        public void Set(ReadOnlySequence<byte> content, IMessageProperties messageProperties)
        {
            this.payload = content;
            this.properties = messageProperties;
            this.isSet = true;
        }

        public void Reset()
        {
            this.payload = ReadOnlySequence<byte>.Empty;
            this.properties = null;
            this.isSet = false;
        }

        public IMessageProperties Properties
        {
            get
            {
                this.ValidateState();
                return this.properties;
            }
        }

        public T GetContent<T>()
        {
            this.ValidateState();
            var serializer = this.serializerFactory.Get(this.properties);
            return serializer.Deserialize<T>(this.payload);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateState()
        {
            if (!this.isSet)
            {
                throw new InvalidOperationException("Cannot get content outsize of message context");
            }
        }
    }
}