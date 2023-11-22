using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace RabbitMQ.Next.Publisher.Attributes;

internal static class PublisherAttributes
{
    private static readonly ConcurrentDictionary<Assembly, IReadOnlyList<MessageAttributeBase>> AssemblyAttributesMap = new();
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<MessageAttributeBase>> TypeAttributesMap = new();

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


    public static void Apply(IMessageBuilder message)
    {
        var typeAttributes = TypeAttributesMap.GetOrAdd(message.ClrType, TypeAttributesFactory);
        ApplyAttributes(message, typeAttributes);

        var assemblyAttributes = AssemblyAttributesMap.GetOrAdd(message.ClrType.Assembly, AssemblyAttributesFactory);
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