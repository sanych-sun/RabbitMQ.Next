using System.Collections.Generic;
using System.Threading;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization
{
    public class StaticFormatterSource : IFormatterSource
    {
        private readonly ReaderWriterLockSlim sync = new ReaderWriterLockSlim();
        private readonly List<IFormatter> formatters = new List<IFormatter>();

        public void Register(IFormatter formatter)
        {
            this.sync.EnterWriteLock();
            try
            {
                this.formatters.Add(formatter);
            }
            finally
            {
                this.sync.ExitWriteLock();
            }
        }

        public IFormatter GetFormatter<TContent>()
        {
            this.sync.EnterReadLock();

            try
            {
                foreach (var item in this.formatters)
                {
                    if (item.CanHandle(typeof(TContent)))
                    {
                        return item;
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