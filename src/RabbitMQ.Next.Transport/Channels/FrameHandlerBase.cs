using System;
using System.Buffers;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next.Transport.Channels
{
    public abstract class FrameHandlerBase
    {
        protected FrameHandlerBase(IMethodRegistry registry)
        {
            this.Registry = registry;
        }

        protected IMethodRegistry Registry { get; }

        protected ReadOnlySequence<byte> ReadMethodId(ReadOnlySequence<byte> payload, out uint methodId)
        {
            if (payload.FirstSpan.Length > sizeof(uint))
            {
                payload.FirstSpan.Read(out methodId);
            }
            else
            {
                Span<byte> buffer = stackalloc byte[sizeof(uint)];
                payload.CopyTo(buffer);
                ((ReadOnlySpan<byte>)buffer).Read(out methodId);
            }

            return payload.Slice(sizeof(uint));
        }

        protected IIncomingMethod ParseMethodArguments(uint methodId, ReadOnlySequence<byte> payload)
        {
            var parser = this.Registry.GetParser(methodId);

            if (payload.IsSingleSegment)
            {
                return parser.ParseMethod(payload.FirstSpan);
            }

            // todo: use memorypool here?
            Span<byte> data = stackalloc byte[(int) payload.Length];
            payload.CopyTo(data);
            return parser.ParseMethod(data);
        }

        protected TMethod ParseArguments<TMethod>(ReadOnlySequence<byte> payload)
            where TMethod: struct, IIncomingMethod
        {
            var parser = this.Registry.GetParser<TMethod>();

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