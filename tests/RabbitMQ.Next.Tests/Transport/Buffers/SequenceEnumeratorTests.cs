using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using RabbitMQ.Next.Transport.Buffers;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Buffers
{
    public class SequenceEnumeratorTests
    {
        [Theory]
        [MemberData(nameof(EnumerationTestCases))]
        public void EnumerationTests(params byte[][] chunks)
        {
            var sequence = BuildSequence(chunks);
            var enumerator = new SequenceEnumerator<byte>(sequence);

            var result = new List<byte[]>();
            while (enumerator.MoveNext())
            {
                result.Add(enumerator.Current.ToArray());
            }

            Assert.Equal(chunks, result.ToArray());
        }

        [Theory]
        [MemberData(nameof(EnumerationTestCases))]
        public void CurrentThrowsBeforeEnumeration(byte[][] chunks)
        {
            var sequence = BuildSequence(chunks);
            var enumerator = new SequenceEnumerator<byte>(sequence);

            Assert.Throws<InvalidOperationException>(() => enumerator.Current);
            Assert.Throws<InvalidOperationException>(() => ((IEnumerator)enumerator).Current);
        }

        [Theory]
        [MemberData(nameof(EnumerationTestCases))]
        public void CurrentThrowsAfterEnumeration(byte[][] chunks)
        {
            var sequence = BuildSequence(chunks);
            var enumerator = new SequenceEnumerator<byte>(sequence);

            while(enumerator.MoveNext())
            {}

            Assert.Throws<InvalidOperationException>(() => enumerator.Current);
            Assert.Throws<InvalidOperationException>(() => ((IEnumerator)enumerator).Current);
        }

        [Theory]
        [MemberData(nameof(EnumerationTestCases))]
        public void ResetEnumeration(byte[][] chunks)
        {
            var sequence = BuildSequence(chunks);
            var enumerator = new SequenceEnumerator<byte>(sequence);

            var firstCount = 0;
            while (enumerator.MoveNext())
            {
                firstCount++;
            }

            enumerator.Reset();
            var secondCount = 0;
            while (enumerator.MoveNext())
            {
                secondCount++;
            }

            Assert.Equal(firstCount, secondCount);
        }

        [Theory]
        [MemberData(nameof(EnumerationTestCases))]
        public void IsLastEnumeration(byte[][] chunks)
        {
            var sequence = BuildSequence(chunks);
            var enumerator = new SequenceEnumerator<byte>(sequence);

            enumerator.MoveNext();
            for (var i = 1; i < chunks.Length; i++)
            {
                Assert.False(enumerator.IsLast);
                enumerator.MoveNext();
            }

            Assert.True(enumerator.IsLast);
        }

        [Fact]
        public void DisposedEnumeratorThrows()
        {
            var sequence = BuildSequence<byte>(null);
            var enumerator = new SequenceEnumerator<byte>(sequence);

            enumerator.Dispose();

            Assert.Throws<ObjectDisposedException>(() => enumerator.Current);
            Assert.Throws<ObjectDisposedException>(() => enumerator.IsLast);
            Assert.Throws<ObjectDisposedException>(() => enumerator.MoveNext());
            Assert.Throws<ObjectDisposedException>(() => enumerator.Reset());
        }

        [Fact]
        public void CanDisposeMultipleTimes()
        {
            var sequence = BuildSequence<byte>(null);
            var enumerator = new SequenceEnumerator<byte>(sequence);

            enumerator.Dispose();

            var exception = Record.Exception(() => enumerator.Dispose());
            Assert.Null(exception);
        }
        public static IEnumerable<object[]> EnumerationTestCases()
        {
            yield return new object[]
            {
                new byte[0][]
            };

            yield return new object[]
            {
                new [] { new byte[] { 1, 2, 3} }
            };

            yield return new object[]
            {
                new [] { new byte[] { 1, 2, 3}, new byte[] { 4, 5, 6} }
            };

            yield return new object[]
            {
                new [] { new byte[] { 1, 2, 3}, new byte[] { 4, 5, 6}, new byte[] { 7, 8, 9, 10} }
            };
        }

        private static ReadOnlySequence<T> BuildSequence<T>(params T[][] chunks)
        {
            if (chunks == null || chunks.Length == 0)
            {
                return ReadOnlySequence<T>.Empty;
            }

            var first = new MemorySegment<T>(chunks[0].AsMemory());
            var last = first;

            for(var i = 1; i < chunks.Length; i++)
            {
                last = last.Append(chunks[i].AsMemory());
            }

            return new ReadOnlySequence<T>(first, 0, last, last.Memory.Length);
        }
    }
}