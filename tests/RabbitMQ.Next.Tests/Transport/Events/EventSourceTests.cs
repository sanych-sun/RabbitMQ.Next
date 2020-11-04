using System;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Transport.Events;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Events
{
    public class EventSourceTests
    {
        [Fact]
        public async Task CallAllSubscribers()
        {
            var subscription1 = Substitute.For<ISubscription<DummyEvent>>();
            var subscription2 = Substitute.For<ISubscription<DummyEvent>>();

            var eventSource = new EventSource<DummyEvent>();
            await eventSource.AddSubscription(subscription1);
            await eventSource.AddSubscription(subscription2);

            await eventSource.InvokeAsync(new DummyEvent());

            await subscription1.Received().HandleAsync(Arg.Any<DummyEvent>());
            await subscription1.Received().HandleAsync(Arg.Any<DummyEvent>());
        }

        [Fact]
        public async Task PassEventIntoSubscriber()
        {
            var subscription = Substitute.For<ISubscription<DummyEvent>>();

            var eventSource = new EventSource<DummyEvent>();
            await eventSource.AddSubscription(subscription);

            var evt = new DummyEvent { SomeData = 128 };
            await eventSource.InvokeAsync(evt);

            await subscription.Received().HandleAsync(evt);
        }

        [Fact]
        public async Task CallAllSubscribersAfterFailed()
        {
            var subscription1 = Substitute.For<ISubscription<DummyEvent>>();
            var failedSubscription = Substitute.For<ISubscription<DummyEvent>>();
            failedSubscription.HandleAsync(default).ReturnsForAnyArgs(new ValueTask<bool>(Task.FromException<bool>(new InvalidOperationException())));
            var subscription3 = Substitute.For<ISubscription<DummyEvent>>();

            var eventSource = new EventSource<DummyEvent>();
            await eventSource.AddSubscription(subscription1);
            await eventSource.AddSubscription(failedSubscription);
            await eventSource.AddSubscription(subscription3);

            await eventSource.InvokeAsync(new DummyEvent());

            await subscription1.Received().HandleAsync(Arg.Any<DummyEvent>());
            await subscription3.Received().HandleAsync(Arg.Any<DummyEvent>());
        }

        [Fact]
        public async Task RemoveInactiveSubscriber()
        {
            var subscription = Substitute.For<ISubscription<DummyEvent>>();
            subscription.HandleAsync(default).ReturnsForAnyArgs(new ValueTask<bool>(false));

            var eventSource = new EventSource<DummyEvent>();
            await eventSource.AddSubscription(subscription);

            await eventSource.InvokeAsync(new DummyEvent());
            await subscription.Received().HandleAsync(Arg.Any<DummyEvent>());

            subscription.Received().ClearReceivedCalls();

            await eventSource.InvokeAsync(new DummyEvent());
            await subscription.DidNotReceive().HandleAsync(Arg.Any<DummyEvent>());
        }

        [Fact]
        public async Task ShouldNotThrowOnEmptySubscriptions()
        {
            var eventSource = new EventSource<DummyEvent>();

            var exception = await Record.ExceptionAsync(async () => await eventSource.InvokeAsync(new DummyEvent()));
            Assert.Null(exception);
        }

        [Fact]
        public async Task CanRegisterSubscription()
        {
            var eventSource = new EventSource<DummyEvent>();
            var subscriber = new DummySubscriber();

            eventSource.Subscribe(subscriber, s => s.ProcessEventAsync);

            var evt = new DummyEvent { SomeData = 321 };
            await eventSource.InvokeAsync(evt);

            Assert.Equal(evt, subscriber.LastReceived);
        }

        [Fact]
        public async Task DoNotCallDisposedSubscription()
        {
            var eventSource = new EventSource<DummyEvent>();
            var subscriber = new DummySubscriber();

            var subscription = eventSource.Subscribe(subscriber, s => s.ProcessEventAsync);
            subscription.Dispose();

            var evt = new DummyEvent { SomeData = 321 };
            await eventSource.InvokeAsync(evt);

            Assert.Equal(default, subscriber.LastReceived);
        }
    }
}