using System;
using System.Collections.Generic;

namespace RabbitMQ.Next.TopologyBuilder.Abstractions
{
    public interface IQueueBuilder
    {
        string Name { get; }

        QueueFlags Flags { get; set; }

        void SetArgument(string key, object value);
    }
}