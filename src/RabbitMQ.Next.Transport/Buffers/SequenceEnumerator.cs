using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RabbitMQ.Next.Transport.Buffers
{
    public struct SequenceEnumerator<T> : IEnumerator<ReadOnlyMemory<T>>
    {
        private readonly ReadOnlySequence<T> source;
        private ReadOnlySequence<T>? remaining;
        private bool isDisposed;

        public SequenceEnumerator(ReadOnlySequence<T> source)
        {
            this.source = source;
            this.remaining = null;
            this.isDisposed = false;
        }

        public bool MoveNext()
        {
            this.CheckDisposed();

            if (!this.remaining.HasValue)
            {
                this.remaining = this.source;
            }
            else
            {
                this.remaining = this.remaining.Value.Slice(this.remaining.Value.First.Length);
            }

            return !this.remaining.Value.IsEmpty;
        }

        public void Reset()
        {
            this.CheckDisposed();
            this.remaining = null;
        }

        public ReadOnlyMemory<T> Current
        {
            get
            {
                this.CheckDisposed();
                if (this.remaining?.IsEmpty ?? true)
                {
                    throw new InvalidOperationException("Cannot access Current outside of enumeration operation");
                }

                return this.remaining.Value.First;
            }
        }

        public bool IsLast
        {
            get
            {
                this.CheckDisposed();
                return this.remaining.Value.IsSingleSegment;
            }
        }

        object IEnumerator.Current => this.Current;

        public void Dispose()
        {
            this.isDisposed = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(SequenceEnumerator<T>));
            }
        }
    }
}