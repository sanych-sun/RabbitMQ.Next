using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RabbitMQ.Next.Transport.Messaging
{
    internal struct TableProperty
    {
        private readonly ReadOnlyMemory<byte> bytes;
        private IReadOnlyDictionary<string,object> value;
        private bool isParsed;

        public TableProperty(ReadOnlyMemory<byte> bytes)
        {
            this.bytes = bytes;
            this.value = null;
            this.isParsed = false;
        }

        public IReadOnlyDictionary<string,object> Value
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
            if (this.bytes.IsEmpty)
            {
                return;
            }

            this.bytes.Span.Read(out Dictionary<string,object> val);
            this.value = val;
        }
    }
}