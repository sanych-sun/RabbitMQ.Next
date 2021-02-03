using System.Collections.Generic;
using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    internal static class TransformersExtensions
    {
        public static void Apply<TContent>(this IReadOnlyList<IMessageTransformer> transformers, TContent content, MessageHeader header)
        {
            if (transformers == null)
            {
                return;
            }

            for (var i = 0; i <= transformers.Count; i++)
            {
                transformers[i].Apply(content, header);
            }
        }
    }
}