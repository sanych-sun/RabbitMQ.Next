using System;
using Microsoft.Extensions.ObjectPool;

namespace RabbitMQ.Next.Buffers
{
    internal sealed class ObjectPoolPolicy<T> : PooledObjectPolicy<T>
    {
        private readonly Func<T> creationPolicy;
        private readonly Func<T, bool> resetPolicy;

        public ObjectPoolPolicy(Func<T> creationPolicy, Func<T, bool> resetPolicy)
        {
            this.creationPolicy = creationPolicy;
            this.resetPolicy = resetPolicy;
        }

        public override T Create() => this.creationPolicy();

        public override bool Return(T obj) => this.resetPolicy(obj);
    }
}