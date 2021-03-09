// using System;
// using System.Buffers;
// using System.Diagnostics;
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
// using RabbitMQ.Next.Transport;
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
//             var connection = new Connection(ConnectionString.Create("amqp://test2:test2@localhost:5672/"));
//
//             await connection.ConnectAsync();
//
//             var publisher = connection.NewPublisher(
//                 builder => builder
//                     .UseTransformer(new ApplicationIdTransformer("unittest"))
//                     .UseAttributesTransformer()
//                     .UseFormatter(new DummyFormatter())
//                     .UseFormatter(new StringFormatter())
//                 );
//
//             var sw = Stopwatch.StartNew();
//
//             //for (var i = 0; i < 10000; i++)
//             {
//                 await publisher.PublishAsync("new test", "amq.fanout", flags: PublishFlags.Mandatory);
//             }
//
//             await Task.Delay(100_000);
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
//             var connection = new Connection(ConnectionString.Create("amqp://test2:test2@localhost:5672/"));
//
//             await connection.ConnectAsync();
//
//             var consumer = connection.NewConsumer(
//                 builder => builder
//                     .BindToQueue("test-queue")
//                     .EachMessageAcknowledgement()
//                     .PrefetchCount(5)
//                     .UseFormatter(new StringFormatter())
//                     .AddMessageHandler((message, content) =>
//                     {
//                         this.output.WriteLine(content.GetContent<string>());
//                         return new ValueTask<bool>(true);
//                     })
//             );
//
//             var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(1));
//             await consumer.ConsumeAsync(cancellation.Token);
//
//             await connection.CloseAsync();
//         }
//
//         [Exchange("MyExchange")]
//         [Header("test", "wokrs")]
//         public class DummyClass
//         {
//
//         }
//
//         public class DummyFormatter : IFormatter
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