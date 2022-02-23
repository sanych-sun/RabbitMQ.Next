using System;

namespace RabbitMQ.Next.Exceptions
{
    public class EndPointResolutionException : Exception
    {
        public EndPointResolutionException(Uri endpoint, Exception innerException)
            : base("Cannot resolve endpoint.", innerException)
        {
            this.Endpoint = endpoint;
        }

        public Uri Endpoint { get; }
    }
}