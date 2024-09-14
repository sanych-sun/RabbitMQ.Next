![image](https://user-images.githubusercontent.com/31327136/206628834-f9c52f6f-fc6e-45eb-bd64-c1e27bdcfb9a.png)

Many thanks for [JetBrains](https://jb.gg/OpenSourceSupport) for providing Open Source Development license for best in class IDEs for this project.

# RabbitMQ.Next

[![NuGet](https://img.shields.io/nuget/v/RabbitMQ.Next.svg)](https://www.nuget.org/packages/RabbitMQ.Next)
[![CI](https://github.com/sanych-sun/RabbitMQ.Next/actions/workflows/master.yml/badge.svg)](https://github.com/sanych-sun/RabbitMQ.Next/actions/workflows/master.yml)
[![License MIT](https://img.shields.io/badge/license-MIT-green.svg)](https://github.com/sanych-sun/RabbitMQ.Next/blob/master/LICENSE)

RabbitMQ.Next is an experimental low-level RabbitMQ client for .Net with number of high-level APIs (created from scratch, no dependencies to another libraries, so no conflicts!). The motivation to create the library was to separate protocol-level code into a package that could be used as a base block for high-level API libraries. The second goal was to reduce allocation as much as possible and maintain reasonable performance. And the last it was sound cool to create a library that can work with sockets on low-level :grin:.

# Performance and Allocation

The library was created with idea of the minimal possible allocation. Here is performance and allocations comparison with official dotnet driver:

## Publisher benchmarks [Find more details](https://github.com/sanych-sun/RabbitMQ.Next/blob/master/docs/benchmarks/RabbitMQ.Next.Benchmarks.Publisher.PublisherBenchmarks.md)

![image](https://user-images.githubusercontent.com/31327136/219306147-b6d71333-22a5-4bc1-9a9c-5846432a9a01.png)

## Consumer benchmarks [Find more details](https://github.com/sanych-sun/RabbitMQ.Next/blob/master/docs/benchmarks/RabbitMQ.Next.Benchmarks.Consumer.ConsumerBenchmarks.md)
![image](https://user-images.githubusercontent.com/31327136/219307235-45c94e50-d251-429f-af2e-18a686f120e6.png)

(not that beautiful as publisher one, but I'm working on it)

## Packages
Too much packages instead of a single... explained:
- [RabbitMQ.Next](https://www.nuget.org/packages/RabbitMQ.Next), [RabbitMQ.Next.Abstractions](https://www.nuget.org/packages/RabbitMQ.Next.Abstractions) – core library, contains most of the protocol-level implementations
- [RabbitMQ.Next.TopologyBuilder](https://www.nuget.org/packages/RabbitMQ.Next.TopologyBuilder), [RabbitMQ.Next.TopologyBuilder.Abstractions](https://www.nuget.org/packages/RabbitMQ.Next.TopologyBuilder.Abstractions) – library contains methods to manage exchanges, queues and bindings
- [RabbitMQ.Next.Consumer](https://www.nuget.org/packages/RabbitMQ.Next.Consumer), [RabbitMQ.Next.Consumer.Abstractions](https://www.nuget.org/packages/RabbitMQ.Next.Consumer.Abstractions) – library provides high-level message consumption API
- [RabbitMQ.Next.Publisher](https://www.nuget.org/packages/RabbitMQ.Next.Publisher), [RabbitMQ.Next.Publisher.Abstractions](https://www.nuget.org/packages/RabbitMQ.Next.Publisher.Abstractions) – provides high-level message publishing API
- [RabbitMQ.Next.Publisher.Attributes](https://www.nuget.org/packages/RabbitMQ.Next.Publisher.Attributes) – helper library that allow declarative attribute-based message initialization.

Serializers:
- [RabbitMQ.Next.Serialization.PlainText](https://www.nuget.org/packages/RabbitMQ.Next.Serialization.PlainText) – provides set of formatters for most common types to produce text/plain encoded messages
- [RabbitMQ.Next.Serialization.SystemJson](https://www.nuget.org/packages/RabbitMQ.Next.Serialization.SystemJson) – json formatter that uses System.Text.Json under the hood
- [RabbitMQ.Next.Serialization.MessagePack](https://www.nuget.org/packages/RabbitMQ.Next.Serialization.MessagePack) – formatter for popular MessagePack serializer
- [RabbitMQ.Next.Serialization.NewtonsoftJson](https://www.nuget.org/packages/RabbitMQ.Next.Serialization.NewtonsoftJson) – formatter for popular NewtonsoftJson serializer
- [RabbitMQ.Next.Serialization.Dynamic](https://www.nuget.org/packages/RabbitMQ.Next.Serialization.Dynamic) – formatter for advanced scenarios, when actual serializer should be selected based on the message properties ([example](https://github.com/sanych-sun/RabbitMQ.Next/blob/master/docs/examples/RabbitMQ.Next.Examples.DynamicSerializer/Program.cs))

More API and integration libraries are coming.

## Getting started

First of all have to open the connection to RabbitMQ server:
```c#
using RabbitMQ.Next;
...

var connection = await ConnectionBuilder.Default
    .Endpoint("amqp://guest:password@localhost:5672/")
    .Build();
```

And basically that's it. Now all ready to make some useful stuff.
### Topology builder
RabbitMQ.Next.TopologyBuilder library contains number of methods to manipulate exchanges, queues and bindings. Create an exchange and bind a queue - it's easy with the library. Complete code of Topology Builder example available [here](https://github.com/sanych-sun/RabbitMQ.Next/tree/master/docs/examples/RabbitMQ.Next.Examples.TopologyBuilder).
```c#
using RabbitMQ.Next.TopologyBuilder;
...

await connection.ConfigureAsync(async topology =>
{
  await topology.Exchange.DeclareAsync("my-exchange", ExchangeType.Topic);
  Console.WriteLine("'my-exchange' was created with using library defaults (durable by default)");

  await topology.Exchange.DeclareAsync("my-advanced-exchange", ExchangeType.Topic,
    builder => builder
      .AutoDelete());
  Console.WriteLine("'my-advanced-exchange' was created by explicitly configuring to be auto-delete");

  Console.WriteLine("--------------------------------------------------------------");

  await topology.Queue.DeclareQuorumAsync("my-queue");
  Console.WriteLine("Declare quorum queue named 'my-queue'");

  await topology.Queue.DeclareClassicAsync("my-advanced-queue",
    builder => builder
      .AutoDelete()
      .MaxLength(1000));
  Console.WriteLine("'my-advanced-queue' was created by explicitly configuring to be auto-delete and max-length 1000");

  Console.WriteLine("--------------------------------------------------------------");

  await topology.Queue.BindAsync("my-queue", "my-exchange", "cat");
  await topology.Queue.BindAsync("my-queue", "my-exchange", "dog");
  Console.WriteLine("my-queue was bound to my-exchange by 2 bindings.");
});
```

### Serializers
Message publisher and consumer require to use serializer, so library know how to format and parse message payloads. So far there are 3 serializers supported:
1. RabbitMQ.Next.Serialization.PlainText
2. RabbitMQ.Next.Serialization.SystemJson
3. RabbitMQ.Next.Serialization.MessagePack
4. RabbitMQ.Next.Serialization.NewtonsoftJson
5. RabbitMQ.Next.Serialization.Dynamic (the one allow to choose serializer type basic on the message properties)

Serializer should be defined on the connection level, by using `builder.UseSerializer` method, or via convenient extension methods. In case when different exchanges should use different type of serialization - use DynamicSerializer. 

However, there is no rocket-science to implement other popular formats integration. Please post an issue in the [issue tracker](https://github.com/sanych-sun/RabbitMQ.Next/issues) and I'll consider implementation of provide some code examples for you.

### Message consumer
RabbitMQ.Next.Consumer library let client code to consume messages. Complete example is [here](https://github.com/sanych-sun/RabbitMQ.Next/tree/master/docs/examples/RabbitMQ.Next.Examples.SimpleConsumer)
```c#
using RabbitMQ.Next.Consumer;
...

await using var consumer = connection.Consumer( // IConsumer implements IAsyncDisposable, do not forget to dispose it 
  builder => builder
    .BindToQueue("test-queue")  // It's possible to bind to multiple queues
    .PrefetchCount(10));        // there are some more tweacks could be applied to consumer

// and start message consumption by providing handler and cancellation token
await consumer.ConsumeAsync(async message =>
{
    Console.WriteLine($"Message received via '{message.Exchange}' exchange: {message.Content<string>()}");
}, cancellation.Token);
```

### Message publisher
RabbitMQ.Next.Publisher library let client application to publish messages. Complete code is [here](https://github.com/sanych-sun/RabbitMQ.Next/tree/master/docs/examples/RabbitMQ.Next.Examples.SimplePublisher).

```c#
using RabbitMQ.Next.Publisher;
...

await using var publisher = connection.Publisher("amq.fanout"); // IPublisher implements IAsyncDisposable, do not forget to dispose it

// And that's it, publisher is ready. There also some more tweaks could be applied to the publisher via publisher builder 
// (for example disable Publisher confirms)
await publisher.PublishAsync("Some cool message");

// Also there is optional message builder, that could be used to set message properties
await publisher.PublishAsync("test message", 
  message => message
    .Priority(5)
    .Type("MyDto"));
```
This is how the last message looks like on the server:

![image](https://user-images.githubusercontent.com/31327136/205428054-7e627426-2821-4d5f-a9a9-6d7bdea66ce6.png)



### Message publisher declarative message attributes
RabbitMQ.Next.Publisher.Attributes let client code to initialize message properties from declarative attributes assigned to the DTO class or onto the assembly. This is not replacement for the RabbitMQ.Next.Publisher library, but convenient extension:

This example require to use some serializer that supports objects serialization, for example RabbitMQ.Next.Serialization.SystemJson
```c#
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Attributes;
...

// have to define DTO, and assign attributes on it
[PriorityAttribute(5)]
[TypeAttribute("MyDto")]
public class SampleDto
{
  public string Title { get;set; }
} 

...
// Small ammendments needed to publisher builder:
await using var publisher = connection.Publisher("amq.fanout",
  builder => builder
    .UseAttributesInitializer());  

// and now it's ready for use
await publisher.PublishAsync(new SampleDto { Title = "test message" }); 

```
This is how the message is looks like:

![image](https://user-images.githubusercontent.com/31327136/205427873-1dd7ded5-f636-4bd6-ba62-6f41a66be792.png)


## Contribute

Contributions to the package are always welcome!

- Report any bugs or issues you find on the [issue tracker](https://github.com/sanych-sun/RabbitMQ.Next/issues).
- You can grab the source code at the package's [git repository](https://github.com/sanych-sun/RabbitMQ.Next).
