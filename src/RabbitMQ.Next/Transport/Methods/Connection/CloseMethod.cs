using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    public readonly struct CloseMethod : IIncomingMethod, IOutgoingMethod
    {
        public MethodId MethodId => MethodId.ConnectionClose;

        public CloseMethod(ushort statusCode, string description, MethodId failedMethod)
        {
            this.StatusCode = statusCode;
            this.Description = description;
            this.FailedMethodId = failedMethod;
        }

        public ushort StatusCode { get; }

        public string Description { get; }

        public MethodId FailedMethodId { get; }
    }
}