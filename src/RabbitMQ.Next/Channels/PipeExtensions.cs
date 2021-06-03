using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Channels
{
    internal static class PipeExtensions
    {
        public static async ValueTask<TResult> ReadAsync<TState, TResult>(this PipeReader reader, uint length, TState state, Func<TState, ReadOnlySequence<byte>, TResult> parser, CancellationToken cancellation = default)
        {
            while (!cancellation.IsCancellationRequested)
            {
                var data = await reader.ReadAsync(cancellation);

                if (data.Buffer.Length >= length)
                {
                    var result = parser(state, data.Buffer.Slice(0, length));
                    reader.AdvanceTo(data.Buffer.GetPosition(length));

                    return result;
                }

                if (data.IsCompleted)
                {
                    break;
                }

                reader.AdvanceTo(data.Buffer.Start);
            }

            return default;
        }

        public static async ValueTask<TResult> ReadAsync<TState, TResult>(this PipeReader reader, uint length, TState state, Func<TState, ReadOnlySequence<byte>, ValueTask<TResult>> parser, CancellationToken cancellation = default)
        {
            while (!cancellation.IsCancellationRequested)
            {
                var data = await reader.ReadAsync(cancellation);

                if (data.Buffer.Length >= length)
                {
                    var result = await parser(state, data.Buffer.Slice(0, length));
                    reader.AdvanceTo(data.Buffer.GetPosition(length));

                    return result;
                }

                if (data.IsCompleted)
                {
                    break;
                }

                reader.AdvanceTo(data.Buffer.Start);
            }

            return default;
        }

        public static ValueTask<TResult> ReadAsync<TResult>(this PipeReader reader, uint length, Func<ReadOnlySequence<byte>, TResult> parser, CancellationToken cancellation = default)
            => reader.ReadAsync(length, parser, (state, sequence) => state(sequence), cancellation);

    }
}