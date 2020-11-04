using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Transport.Events;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Events
{
    public class SubscriptionTests
    {
        [Fact]
        public async Task SubscriptionActiveIfTargetAlive()
        {
            var subscriber = new DummySubscriber();
            var subscription = this.MakeSubscription(subscriber);
            
            GC.Collect();

            var result = await subscription.HandleAsync(new DummyEvent());

            Assert.True(result);
        }

        [Fact]
        public async Task SubscriptionDeactivateOnTargetCollected()
        {
            var subscription = this.MakeSubscription();

            GC.Collect();

            var result = await subscription.HandleAsync(new DummyEvent());

            Assert.False(result);
        }

        [Fact]
        public async Task SubscriptionDeactivateOnDispose()
        {
            var subscription = this.MakeSubscription();

            subscription.Dispose();

            var result = await subscription.HandleAsync(new DummyEvent());

            Assert.False(result);
        }

        [Fact]
        public async Task SubscriptionPassEventIntoHandler()
        {
            var subscriber = new DummySubscriber();
            var subscription = this.MakeSubscription(subscriber);
            var evt = new DummyEvent {SomeData = 42};

            var result = await subscription.HandleAsync(evt);

            Assert.Equal(evt, subscriber.LastReceived);
        }

        [Fact]
        public async Task SubscriptionThrows()
        {
            var subscriber = new DummySubscriber();
            var subscription1 = new Subscription<DummySubscriber, DummyEvent>(subscriber, s => s.FailedHandlerAsync);
            var subscription2 = new Subscription<DummySubscriber, DummyEvent>(subscriber, s => s.FailedHandlerInAsync);

            await Assert.ThrowsAnyAsync<Exception>(async () => await subscription1.HandleAsync(new DummyEvent()));
            await Assert.ThrowsAnyAsync<Exception>(async () => await subscription2.HandleAsync(new DummyEvent()));
        }


        private Subscription<DummySubscriber, DummyEvent> MakeSubscription()
        {
            var subscriber = new DummySubscriber();
            return this.MakeSubscription(subscriber);
        }

        private Subscription<DummySubscriber, DummyEvent> MakeSubscription(DummySubscriber subscriber)
        {
            return new Subscription<DummySubscriber, DummyEvent>(subscriber, s => s.ProcessEventAsync);
        }
    }
}