using System;
using System.Collections.Generic;

namespace RabbitMQ.Next.Exceptions
{
    public class EndPointResolutionException : Exception
    {
        public EndPointResolutionException(IReadOnlyDictionary<Uri, Exception> exceptions)
            : base("Cannot establish connection to RabbitMQ cluster. See InnerExceptions for more details.")
        {
            this.InnerExceptions = exceptions;
        }

        public IReadOnlyDictionary<Uri, Exception> InnerExceptions { get; }
    }
}