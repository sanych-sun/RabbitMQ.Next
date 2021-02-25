using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;

namespace RabbitMQ.Next.Publisher.Attributes
{
    internal sealed class AttributeTransformer : IMessageTransformer
    {
        private static readonly ConcurrentDictionary<Assembly, IReadOnlyList<MessageAttributeBase>> AssemblyAttributesMap = new ConcurrentDictionary<Assembly, IReadOnlyList<MessageAttributeBase>>();
        private static readonly ConcurrentDictionary<Type, IReadOnlyList<MessageAttributeBase>> TypeAttributesMap = new ConcurrentDictionary<Type, IReadOnlyList<MessageAttributeBase>>();

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

            var typeAttributes = TypeAttributesMap.GetOrAdd(type, TypeAttributesFactory);
            this.ApplyAttributes(message, typeAttributes);

            var assemblyAttributes = AssemblyAttributesMap.GetOrAdd(type.Assembly, AssemblyAttributesFactory);
            this.ApplyAttributes(message, assemblyAttributes);
        }

        private void ApplyAttributes(IMessageBuilder message, IReadOnlyList<MessageAttributeBase> attributes)
        {
            for (var i = 0; i < attributes.Count; i++)
            {
                attributes[i].Apply(message);
            }
        }
    }
}