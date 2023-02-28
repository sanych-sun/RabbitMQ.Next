using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Publisher.Attributes;

internal sealed class AttributePublishMiddleware : IPublishMiddleware
{
    private static readonly ConcurrentDictionary<Assembly, IReadOnlyList<MessageAttributeBase>> AssemblyAttributesMap = new();
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<MessageAttributeBase>> TypeAttributesMap = new();
    
    private readonly IPublishMiddleware next;

    private static readonly Func<Type, IReadOnlyList<MessageAttributeBase>> TypeAttributesFactory = type =>
    {
        var attributes = type.GetCustomAttributes(typeof(MessageAttributeBase), true);
        return attributes.Length == 0 ? Array.Empty<MessageAttributeBase>() : AsMessageAttributes(attributes);
    };

    private static readonly Func<Assembly, IReadOnlyList<MessageAttributeBase>> AssemblyAttributesFactory = assembly =>
    {
        var attributes = assembly.GetCustomAttributes(typeof(MessageAttributeBase), true);
        return attributes.Length == 0 ? Array.Empty<MessageAttributeBase>() : AsMessageAttributes(attributes);
    };

    public AttributePublishMiddleware(IPublishMiddleware next)
    {
        this.next = next;
    }

    private static IReadOnlyList<MessageAttributeBase> AsMessageAttributes(object[] attributes)
    {
        var typed = new MessageAttributeBase[attributes.Length];
        for (var i = 0; i < typed.Length; i++)
        {
            typed[i] = (MessageAttributeBase) attributes[i];
        }

        return typed;
    }

    public ValueTask InvokeAsync<TContent>(TContent content, IMessageBuilder message, CancellationToken cancellation)
    {
        var type = typeof(TContent);

        var typeAttributes = TypeAttributesMap.GetOrAdd(type, TypeAttributesFactory);
        ApplyAttributes(message, typeAttributes);

        var assemblyAttributes = AssemblyAttributesMap.GetOrAdd(type.Assembly, AssemblyAttributesFactory);
        ApplyAttributes(message, assemblyAttributes);

        return this.next.InvokeAsync(content, message, cancellation);
    }

    private static void ApplyAttributes(IMessageBuilder message, IReadOnlyList<MessageAttributeBase> attributes)
    {
        for (var i = 0; i < attributes.Count; i++)
        {
            attributes[i].Apply(message);
        }
    }
}