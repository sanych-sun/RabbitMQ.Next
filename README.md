[![NuGet](https://img.shields.io/nuget/v/RabbitMQ.Next.svg)](https://www.nuget.org/packages/RabbitMQ.Next)
[![CI](https://github.com/sanych-sun/RabbitMQ.Next/actions/workflows/master.yml/badge.svg)](https://github.com/sanych-sun/RabbitMQ.Next/actions/workflows/master.yml)
[![License MIT](https://img.shields.io/badge/license-MIT-green.svg)](https://github.com/sanych-sun/RabbitMQ.Next/blob/master/LICENSE)

# RabbitMQ.Next

RabbitMQ.Next is an experimental low-level RabbitMQ client for .Net with number of high-level APIs (created from scratch, no dependencies to another libraries, so no conflicts!). The motivation to create the library was to separate protocol-level code into a package that could be used as a base block for high-level API libraries. The second goal was to reduce allocation as much as possible and maintain reasonable performance. And the last it was sound cool to create a library that can work with sockets on low-level :grin:.

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
```c#
using RabbitMQ.Next;
...

var connection = await ConnectionBuilder.Default
    .Endpoint("amqp://guest:password@localhost:5672/")
    .ConnectAsync();
```

And basically that's it. Now all ready to make some useful stuff.
### Topology builder
RabbitMQ.Next.TopologyBuilder library contains number of methods to manipulate exchanges, queues and bindings. Create an exchange and bind a queue - it's easy with the library. Complete code of Topology Builder example available [here](https://github.com/sanych-sun/RabbitMQ.Next/tree/master/docs/examples/RabbitMQ.Next.Examples.TopologyBuilder).
```c#
using RabbitMQ.Next.TopologyBuilder;
...

await connection.ExchangeDeclareAsync("my-exchange", ExchangeType.Topic); // Create topic named 'my-exchange'
await connection.ExchangeDeclareAsync("my-advanced-exchange", ExchangeType.Topic, 
  builder => builder  // It's possible to tweak exchange parameters using exchange builder
    .Transient());

await connection.QueueDeclareAsync("my-queue"); // Create queue named "my-queue"
await connection.QueueDeclareAsync("my-advanced-queue", 
  builder => builder  // To adjust queue properties use queue builder
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

### Message consumer
RabbitMQ.Next.Consumer library let client code to consume messages. Complete example is [here](https://github.com/sanych-sun/RabbitMQ.Next/tree/master/docs/examples/RabbitMQ.Next.Examples.SimpleConsumer)
```c#
using RabbitMQ.Next.Consumer;
using RabbitMQ.Next.Serialization.PlainText;
...

var consumer = connection.Consumer(
  builder => builder
    .BindToQueue("test-queue")  // It's possible to bind to multiple queues
    .PrefetchCount(10)          // there are some more tweacks could be applied to consumer
    .UsePlainTextSerializer()); // and we need serializer

// and start message consumption by providing handler and cancellation token
await consumer.ConsumeAsync(async message =>
{
    Console.WriteLine($"Message received via '{message.Exchange}' exchange: {message.Content<string>()}");
} ,cancellation.Token);
```

### Message publisher
RabbitMQ.Next.Publisher library let client application to publish messages. Complete code is [here](https://github.com/sanych-sun/RabbitMQ.Next/tree/master/docs/examples/RabbitMQ.Next.Examples.SimplePublisher).

```c#
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Serialization.PlainText;
...

var publisher = connection.Publisher("amq.fanout",
  builder => builder
    .UsePlainTextSerializer()); // It's required to specify the serializer, so library know how to format payload.

// And that's it, publisher is ready. There also some more tweaks could be applied to the publisher via publisher builder 
// (for example disable Publisher confirms)
await publisher.PublishAsync("Some cool message");

// Also there is optional message builder, that could be used to set message properties
await publisher.PublishAsync("test message", message => message
        .Priority(5)
        .Type("MyDto"));
```
This is how the last message looks like on the server:

![image](https://user-images.githubusercontent.com/31327136/205428054-7e627426-2821-4d5f-a9a9-6d7bdea66ce6.png)



### Message publisher declarative message attributes
RabbitMQ.Next.Publisher.Attributes let client code to initialize message properties from declarative attributes assigned to the DTO class or onto the assembly. This is not replacement for the RabbitMQ.Next.Publisher library, but convinient extension:

This example require to use some serializer that supports objects serialization, for example RabbitMQ.Next.Serialization.SystemJson
```c#
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Attributes;
using RabbitMQ.Next.Serialization.SystemJson; 
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
var publisher = connection.Publisher("amq.fanout",
    builder => builder
        .UseSystemJsonSerializer()
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
