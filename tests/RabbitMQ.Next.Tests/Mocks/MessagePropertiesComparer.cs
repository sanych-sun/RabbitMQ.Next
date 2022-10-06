using System;
using System.Collections.Generic;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Tests.Mocks;

public class MessagePropertiesComparer : IEqualityComparer<IMessageProperties>
{
    public bool Equals(IMessageProperties x, IMessageProperties y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;

        var result =
            x.ContentType == y.ContentType
            && x.ContentEncoding == y.ContentEncoding
            && Helpers.DictionaryEquals(x.Headers, y.Headers)
            && x.DeliveryMode == y.DeliveryMode
            && x.Priority == y.Priority
            && x.CorrelationId == y.CorrelationId
            && x.ReplyTo == y.ReplyTo
            && x.Expiration == y.Expiration
            && x.MessageId == y.MessageId
            && Nullable.Equals(x.Timestamp, y.Timestamp)
            && x.Type == y.Type
            && x.UserId == y.UserId
            && x.ApplicationId == y.ApplicationId;

        return result;
    }

    public int GetHashCode(IMessageProperties obj)
    {
        var hashCode = new HashCode();
        hashCode.Add(obj.ContentType);
        hashCode.Add(obj.ContentEncoding);
        hashCode.Add(obj.Headers);
        hashCode.Add((int) obj.DeliveryMode);
        hashCode.Add(obj.Priority);
        hashCode.Add(obj.CorrelationId);
        hashCode.Add(obj.ReplyTo);
        hashCode.Add(obj.Expiration);
        hashCode.Add(obj.MessageId);
        hashCode.Add(obj.Timestamp);
        hashCode.Add(obj.Type);
        hashCode.Add(obj.UserId);
        hashCode.Add(obj.ApplicationId);
        return hashCode.ToHashCode();
    }
}