using System;

namespace RabbitMQ.Next.Messaging
{
    public interface IContent: IMessageProperties, IDisposable
    {
        T Content<T>();        
    }
}