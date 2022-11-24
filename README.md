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

## Contribute

Contributions to the package are always welcome!

- Report any bugs or issues you find on the [issue tracker](https://github.com/sanych-sun/RabbitMQ.Next/issues).
- You can grab the source code at the package's [git repository](https://github.com/sanych-sun/RabbitMQ.Next).
