using System;
using System.Runtime.CompilerServices;

namespace RabbitMQ.Next.Transport.Messaging
{
    internal struct StringProperty
    {
        private readonly ReadOnlyMemory<byte> bytes;
        private string value;
        private bool isParsed;

        public StringProperty(ReadOnlyMemory<byte> bytes)
        {
            this.bytes = bytes;
            this.value = null;
            this.isParsed = false;
        }

        public string Value
        {
            get
            {
                this.EnsureParsed();
                return this.value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureParsed()
        {
            if (this.isParsed)
            {
                return;
            }

            this.isParsed = true;
            if (!this.bytes.IsEmpty)
            {
                this.bytes.Span.Read(out this.value);
            }
        }
    }
}