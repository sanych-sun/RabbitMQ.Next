using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Channel
{
    public readonly struct CloseMethod : IIncomingMethod, IOutgoingMethod
    {
        public CloseMethod(ushort statusCode, string description, uint failedMethod)
        {
            this.StatusCode = statusCode;
            this.Description = description;
            this.FailedMethodId = failedMethod;
        }

        public uint Method => (uint) MethodId.ChannelClose;

        public ushort StatusCode { get; }

        public string Description { get; }

        public uint FailedMethodId { get; }
    }
}