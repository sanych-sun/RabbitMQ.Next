[![CI](https://github.com/sanych-sun/RabbitMQ.Next/actions/workflows/master.yml/badge.svg)](https://github.com/sanych-sun/RabbitMQ.Next/actions/workflows/master.yml)

# RabbitMQ.Next

RabbitMQ.Next is an experimental low-level RabbitMQ client for .Net with number of high-level APIs. The motivation to create the library was to separate protocol-level code into a package that could be used as a base block for high-level API libraries. And it was sound cool to create a library that can work with sockets on low-level :grin:.

## Packages
Too much packages instead of a single... explained:
- RabbitMQ.Next, RabbitMQ.Next.Abstractions – core library, contains most of the protocol-level implementations
- RabbitMQ.Next.TopologyBuilder – library contains methods to manage exchanges, queues and bindings
- RabbitMQ.Next.Consumer, RabbitMQ.Next.Consumer.Abstractions – library provides high-level message consumption API
- RabbitMQ.Next.Publisher, RabbitMQ.Next.Publisher.Abstractions – provides high-level message publishing API
- RabbitMQ.Next.Publisher.Attributes – helper library that allow declarative attribute-based message initialization.

Serializers:
- RabbitMQ.Next.Serialization.PlainText – provides set of formatters for most common types to produce text/plain encoded messages
- RabbitMQ.Next.Serialization.SystemJson – json formatter that uses System.Text.Json under the hood
- RabbitMQ.Next.Serialization.MessagePack – formatter for popular MessagePack serializer

More API and integration libraries are coming.

## Getting started

First of all have to open the connection to RabbitMQ server:
```
using RabbitMQ.Next;
...

var connection = await ConnectionBuilder.Default
    .Endpoint("amqp://guest:password@localhost:5672/")
    .ConnectAsync();
```

And basically that's it. Now all ready to make some useful stuff.
### Topology builder
RabbitMQ.Next.TopologyBuilder library contains number of methods to manipulate exchanges, queues and bindings. Create an exchange and bind a queue - it's easy with the library. Complete code of Topology Builder example available [here](https://github.com/sanych-sun/RabbitMQ.Next/tree/master/docs/examples/RabbitMQ.Next.Examples.TopologyBuilder).
```
using RabbitMQ.Next.TopologyBuilder;
...

await connection.ExchangeDeclareAsync("my-exchange", ExchangeType.Topic); // Create topic named 'my-exchange' using default settings (durable)
await connection.ExchangeDeclareAsync("my-advanced-exchange", ExchangeType.Topic, // It's possible to twick exchange parameters using exchange builder
    builder => builder
        .Transient());

await connection.QueueDeclareAsync("my-queue"); // Create queue named "my-queue" using default settings (durable)
await connection.QueueDeclareAsync("my-advanced-queue", // To adjust queue properties use queue builder
    builder => builder
        .AutoDelete()
        .WithMaxLength(1000));

await connection.QueueBindAsync("my-queue", "my-exchange", // And finaly bind queue to the exchange.
    builder => builder
        .RoutingKey("cat")
        .RoutingKey("dog"));
```

### Serializers
Message publisher and consumer require to use serializer, so library know how to format and parse message payloads. So far there are 3 serializers supported:
1. RabbitMQ.Next.Serialization.PlainText
2. RabbitMQ.Next.Serialization.SystemJson
3. RabbitMQ.Next.Serialization.MessagePack

However there is no rocket-science to implement other popular formats integration. Please post an issue in the [issue tracker](https://github.com/sanych-sun/RabbitMQ.Next/issues) and I'll consider implementation of provide some code examples for you.

### Message publisher
RabbitMQ.Next.Publisher library let client application to publish messages. Complete code is [here](https://github.com/sanych-sun/RabbitMQ.Next/tree/master/docs/examples/RabbitMQ.Next.Examples.SimplePublisher).

```
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Serialization.PlainText;
...

var publisher = connection.Publisher("amq.fanout",
    builder => builder.UsePlainTextSerializer()); // It's required to specify the serializer, so library know how to format payload.

// And that's it, publisher is ready. There also some more tweaks could be applied to the publisher via publisher builder (for example disable Publisher confirms)
await publisher.PublishAsync("Some cool message");

```

### Message consumer
RabbitMQ.Next.Consumer library let client code to consume messages. Complete example is [here](https://github.com/sanych-sun/RabbitMQ.Next/tree/master/docs/examples/RabbitMQ.Next.Examples.SimpleConsumer)
```
using RabbitMQ.Next.Consumer;
using RabbitMQ.Next.Serialization.PlainText;
...

var consumer = connection.Consumer(
    builder => builder
        .BindToQueue("test-queue")  // It's possible to bind to multiple queues
        .PrefetchCount(10)          // there are some more tweacks could be applied to consumer
        .UsePlainTextSerializer()); // and again we need serializer

// and start message consumption by providing handler and cancellation token
await consumer.ConsumeAsync(async message =>
{
    Console.WriteLine($"[{DateTimeOffset.Now.TimeOfDay}] Message received via '{message.Exchange}' exchange: {message.Content<string>()}");
} ,cancellation.Token);
```

## Contribute

Contributions to the package are always welcome!

- Report any bugs or issues you find on the [issue tracker](https://github.com/sanych-sun/RabbitMQ.Next/issues).
- You can grab the source code at the package's [git repository](https://github.com/sanych-sun/RabbitMQ.Next).
