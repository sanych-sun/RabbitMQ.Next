using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Transport
{
    internal static class PipeExtensions
    {
        public static async ValueTask<TResult> ReadAsync<TResult>(this PipeReader reader, uint length, Func<ReadOnlySequence<byte>, TResult> parser, CancellationToken cancellation = default)
        {
            while (true)
            {
                var data = await reader.ReadAsync(cancellation);

                if (data.Buffer.Length >= length)
                {
                    var result = parser(data.Buffer.Slice(0, length));
                    reader.AdvanceTo(data.Buffer.GetPosition(length));

                    return result;
                }

                if (data.IsCompleted)
                {
                    return default;
                }

                reader.AdvanceTo(data.Buffer.Start);
            }
        }
    }
}