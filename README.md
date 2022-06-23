[![CI](https://github.com/sanych-sun/RabbitMQ.Next/actions/workflows/master.yml/badge.svg)](https://github.com/sanych-sun/RabbitMQ.Next/actions/workflows/master.yml)

# RabbitMQ.Next

RabbitMQ.Next is an experimental low-level RabbitMQ client for .Net with number of high-level APIs. The motivation to create the library was to separate protocol-level code into a package that could be used as a base block for high-level API libraries. And it was sound cool to create a library that can work with sockets on low-level :grin:.

## Packages
Too much packages instead of a single... explained:
- RabbitMQ.Next, RabbitMQ.Next.Abstractions – core library, contains most of the protocol-level implementations
- RabbitMQ.Next.TopologyBuilder – library contains methods to manage exchanges, queues and bindings
- RabbitMQ.Next.Consumer – library provides high-level event consumption API
- RabbitMQ.Next.Consumer.Abstractions – contract library for the RabbitMQ.Next.Consumer
- RabbitMQ.Next.Publisher – provides high-level event publishing API
- RabbitMQ.Next.Publisher.Abstractions – contract library for the RabbitMQ.Next.Publisher
- RabbitMQ.Next.Publisher.Attributes – helper library that allow declarative attribute-based message initialization.

Serializers:
- RabbitMQ.Next.Serialization.PlainText – provides set of formatters for most common types to produce text/plain encoded messages
- RabbitMQ.Next.Serialization.SystemJson – json formatter that uses System.Text.Json under the hood
- RabbitMQ.Next.Serialization.MessagePack – formatter for popular MessagePack serializer

More API and integration libraries are coming.
