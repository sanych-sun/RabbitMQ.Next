// using System;
// using System.Buffers;
// using System.Diagnostics;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using RabbitMQ.Next.Consumer;
// using RabbitMQ.Next.Consumer.Abstractions;
// using RabbitMQ.Next.Publisher;
// using RabbitMQ.Next.Publisher.Abstractions;
// using RabbitMQ.Next.Publisher.Attributes;
// using RabbitMQ.Next.Publisher.Transformers;
// using RabbitMQ.Next.Serialization.Abstractions;
// using RabbitMQ.Next.Serialization.Formatters;
// using RabbitMQ.Next;
// using Xunit;
// using Xunit.Abstractions;
//
// namespace RabbitMQ.Next.Tests
// {
//     public class UnitTest1
//     {
//         private readonly ITestOutputHelper output;
//
//         public UnitTest1(ITestOutputHelper output)
//         {
//             this.output = output;
//         }
//
//         [Fact]
//         public async Task Test1()
//         {
//             var connection = ConnectionBuilder
//                 .Create("amqp://test2:test2@localhost:5672/")
//                 .UseDefaults()
//                 .Build();
//
//             await connection.ConnectAsync();
//
//             var publisher = connection.NewPublisher("MyExchange",
//                 builder => builder
//                     .UseTransformer(new ApplicationIdTransformer("unittest"))
//                     .UseAttributesTransformer()
//                     .UseFormatter(new DummyFormatter())
//                     .UseFormatter(new StringTypeFormatter())
//                     .PublisherConfirms()
//                 );
//
//             var sw = Stopwatch.StartNew();
//
//             for (var i = 0; i < 1000; i++)
//             {
//                 await publisher.PublishAsync($"new test {i}", flags: PublishFlags.Mandatory);
//             }
//
//             await publisher.CompleteAsync();
//
//             sw.Stop();
//
//             this.output.WriteLine(sw.ElapsedMilliseconds.ToString());
//
//             await connection.CloseAsync();
//         }
//
//         [Fact]
//         public async Task TestConsumer()
//         {
//             //var connection = new Connection(ConnectionString.Create("amqp://rpeesesf:naQF5gZbGA9GzNHkSKE4QxwBt__Lsmu-@beaver.rmq.cloudamqp.com/rpeesesf"));
//             var connection = ConnectionBuilder
//                 .Create("amqp://test1:test1@localhost:5672/")
//                 .UseDefaults()
//                 .Build();
//
//             await connection.ConnectAsync();
//
//             var consumer = connection.NewConsumer(
//                 builder => builder
//                     .BindToQueue("test-queue")
//                     .EachMessageAcknowledgement()
//                     .PrefetchCount(5)
//                     .UseFormatter(new StringTypeFormatter())
//                     .AddMessageHandler((message, properties, content) =>
//                     {
//                         return new ValueTask<bool>(true);
//                     })
//             );
//
//             var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(10));
//
//             await consumer.ConsumeAsync(cancellation.Token);
//
//             await connection.CloseAsync();
//         }
//
//         [Header("test", "wokrs")]
//         public class DummyClass
//         {
//
//         }
//
//         public class DummyFormatter : ITypeFormatter
//         {
//             public bool CanHandle(Type type) => type == typeof(DummyClass);
//
//             public void Format<TContent>(TContent content, IBufferWriter<byte> writer)
//             {
//                 // writer.Write(new byte[] { 55 });
//             }
//
//             public TContent Parse<TContent>(ReadOnlySequence<byte> bytes)
//             {
//                 if (new DummyClass() is TContent res)
//                 {
//                     return res;
//                 }
//
//                 return default;
//             }
//         }
//     }
// }