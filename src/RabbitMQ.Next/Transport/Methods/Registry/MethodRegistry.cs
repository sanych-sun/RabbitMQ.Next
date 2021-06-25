using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Registry
{
    internal sealed class MethodRegistry : IMethodRegistry
    {
        private readonly IReadOnlyDictionary<MethodId, IMethodRegistration> methods;
        private readonly IReadOnlyDictionary<Type, IMethodRegistration> types;

        public MethodRegistry(IReadOnlyList<IMethodRegistration> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var methodMap = new Dictionary<MethodId, IMethodRegistration>();
            var typeMap = new Dictionary<Type, IMethodRegistration>();
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                methodMap[item.MethodId] = item;
                typeMap[item.Type] = item;
            }

            this.methods = methodMap;
            this.types = typeMap;
        }

        public bool HasContent(MethodId methodId) => this.GetMethod(methodId).HasContent;

        public Type GetMethodType(MethodId methodId) => this.GetMethod(methodId).Type;

        public MethodId GetMethodId<TMethod>() where TMethod : struct, IMethod => this.GetMethod(typeof(TMethod)).MethodId;

        public IMethodParser<TMethod> GetParser<TMethod>()
            where TMethod : struct, IIncomingMethod
        {
            var registration = this.GetMethod(typeof(TMethod));
            if (registration.Parser is IMethodParser<TMethod> item)
            {
                return item;
            }

            return null;
        }

        public IMethodParser GetParser(MethodId methodId) => (IMethodParser)this.GetMethod(methodId).Parser;

        public IMethodFormatter<TMethod> GetFormatter<TMethod>()
            where TMethod : struct, IOutgoingMethod
        {
            var registration = this.GetMethod(typeof(TMethod));
            if (registration.Formatter is IMethodFormatter<TMethod> item)
            {
                return item;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IMethodRegistration GetMethod(MethodId methodId)
        {
            if (this.methods.TryGetValue(methodId, out var info))
            {
                return info;
            }

            throw new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IMethodRegistration GetMethod(Type type)
        {
            if (this.types.TryGetValue(type, out var info))
            {
                return info;
            }

            throw new NotSupportedException();
        }
    }
}