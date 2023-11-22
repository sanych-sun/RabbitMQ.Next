using System;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Publisher;

internal class ContentWrapper<TContent>: IContentAccessor
{
    private readonly TContent content;

    public ContentWrapper(TContent content)
    {
        this.content = content;
    }

    public void Dispose()
    {
    }

    public T Get<T>()
    {
        if (this.content is T result)
        {
            return result;
        }

        return (T)Convert.ChangeType(this.content, typeof(T));
    }
}