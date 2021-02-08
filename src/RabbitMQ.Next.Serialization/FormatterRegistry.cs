using System;
using System.Collections.Generic;
using System.Threading;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization
{
    public class FormatterRegistry
    {
        private readonly ReaderWriterLockSlim sync = new ReaderWriterLockSlim();
        private readonly Dictionary<Type, IFormatterWrapper> formatters = new Dictionary<Type, IFormatterWrapper>();

        public void Register<TType>(IFormatter<TType> formatter)
        {
            this.sync.EnterWriteLock();
            try
            {
                this.formatters.Add(typeof(TType), new FormatterWrapper<TType>(formatter));
            }
            finally
            {
                this.sync.ExitWriteLock();
            }
        }

        internal IFormatterWrapper GetFormatter<TContent>()
        {
            this.sync.EnterReadLock();

            try
            {
                if (this.formatters.TryGetValue(typeof(TContent), out var result))
                {
                    return result;
                }

                foreach (var formatter in this.formatters)
                {
                    if (formatter.Key.IsAssignableFrom(typeof(TContent)))
                    {
                        return formatter.Value;
                    }
                }
            }
            finally
            {
                this.sync.ExitReadLock();
            }

            return null;
        }
    }
}