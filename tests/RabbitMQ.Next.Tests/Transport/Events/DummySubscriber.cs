using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Tests.Transport.Events
{
    public class DummySubscriber
    {
        public ValueTask ProcessEventAsync(DummyEvent evt)
        {
            this.LastReceived = evt;
            return default;
        }

        public ValueTask FailedHandlerAsync(DummyEvent evt)
        {
            throw new InvalidOperationException();
        }

        public async ValueTask FailedHandlerInAsync(DummyEvent evt)
        {
            await Task.Delay(100);
            throw new InvalidOperationException();
        }

        public DummyEvent LastReceived { get; private set; }
    }
}