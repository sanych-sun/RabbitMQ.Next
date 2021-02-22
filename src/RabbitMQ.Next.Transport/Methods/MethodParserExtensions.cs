using System;
using System.Buffers;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods
{
    public static class MethodParserExtensions
    {
        public static IIncomingMethod ParseMethod(this IMethodParser parser, ReadOnlySequence<byte> payload)
        {
            if (payload.IsSingleSegment)
            {
                return parser.ParseMethod(payload.FirstSpan);
            }

            // todo: use memorypool here?
            Span<byte> data = stackalloc byte[(int) payload.Length];
            payload.CopyTo(data);
            return parser.ParseMethod(data);
        }

        public static  TMethod Parse<TMethod>(this IMethodParser<TMethod> parser, ReadOnlySequence<byte> payload)
            where TMethod: struct, IIncomingMethod
        {
            if (payload.IsSingleSegment)
            {
                return parser.Parse(payload.FirstSpan);
            }

            // todo: use memorypool here?
            Span<byte> data = stackalloc byte[(int) payload.Length];
            payload.CopyTo(data);
            return parser.Parse(data);
        }
    }
}