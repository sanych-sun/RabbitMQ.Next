using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Registry
{
    internal sealed class MethodRegistry : IMethodRegistry
    {
        private readonly IReadOnlyDictionary<uint, IMethodRegistration> methods;
        private readonly IReadOnlyDictionary<Type, IMethodRegistration> types;

        public MethodRegistry(IReadOnlyList<IMethodRegistration> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var methodMap = new Dictionary<uint, IMethodRegistration>();
            var typeMap = new Dictionary<Type, IMethodRegistration>();
            foreach (var item in items)
            {
                methodMap[item.MethodId] = item;
                typeMap[item.ImplementationType] = item;
            }

            this.methods = methodMap;
            this.types = typeMap;
        }

        public bool HasContent(uint methodId) => this.GetMethod(methodId).HasContent;

        public Type GetMethodType(uint methodId) => this.GetMethod(methodId).ImplementationType;

        public uint GetMethodId<TMethod>() where TMethod : struct, IMethod => this.GetMethod(typeof(TMethod)).MethodId;

        public IMethodParser<TMethod> GetParser<TMethod>()
            where TMethod : struct, IIncomingMethod
        {
            var registration = this.GetMethod(typeof(TMethod));
            if (registration.Parser is IMethodParser<TMethod> item)
            {
                return item;
            }

            throw new IndexOutOfRangeException();
        }

        public IMethodParser GetParser(uint methodId) => (IMethodParser)this.GetMethod(methodId).Parser;

        public IMethodFormatter<TMethod> GetFormatter<TMethod>()
            where TMethod : struct, IOutgoingMethod
        {
            var registration = this.GetMethod(typeof(TMethod));
            if (registration.Formatter is IMethodFormatter<TMethod> item)
            {
                return item;
            }

            throw new IndexOutOfRangeException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IMethodRegistration GetMethod(uint methodId)
        {
            if (this.methods.TryGetValue(methodId, out var info))
            {
                return info;
            }

            throw new IndexOutOfRangeException();
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