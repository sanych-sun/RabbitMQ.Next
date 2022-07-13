using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace RabbitMQ.Next.Publisher.Attributes;

internal sealed class AttributeInitializer : IMessageInitializer
{
    private readonly ConcurrentDictionary<Assembly, IReadOnlyList<MessageAttributeBase>> assemblyAttributesMap = new();
    private readonly ConcurrentDictionary<Type, IReadOnlyList<MessageAttributeBase>> typeAttributesMap = new();

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

    private static IReadOnlyList<MessageAttributeBase> AsMessageAttributes(object[] attributes)
    {
        var typed = new MessageAttributeBase[attributes.Length];
        for (var i = 0; i < typed.Length; i++)
        {
            typed[i] = (MessageAttributeBase) attributes[i];
        }

        return typed;
    }

    public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
    {
        var type = typeof(TPayload);

        var typeAttributes = this.typeAttributesMap.GetOrAdd(type, TypeAttributesFactory);
        ApplyAttributes(message, typeAttributes);

        var assemblyAttributes = this.assemblyAttributesMap.GetOrAdd(type.Assembly, AssemblyAttributesFactory);
        ApplyAttributes(message, assemblyAttributes);
    }

    private static void ApplyAttributes(IMessageBuilder message, IReadOnlyList<MessageAttributeBase> attributes)
    {
        for (var i = 0; i < attributes.Count; i++)
        {
            attributes[i].Apply(message);
        }
    }
}