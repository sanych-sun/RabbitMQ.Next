// using System;
// using System.Buffers;
// using System.Diagnostics;
// using System.Text;
// using System.Threading;
// using System.Threading.Tasks;
// using RabbitMQ.Next.Consumer;
// using RabbitMQ.Next.Consumer.Abstractions;
// using RabbitMQ.Next.Abstractions;
// using RabbitMQ.Next.Publisher;
// using RabbitMQ.Next.Publisher.Abstractions;
// using RabbitMQ.Next.Publisher.Attributes;
// using RabbitMQ.Next.Serialization.PlainText;
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
//             await using var connection = await ConnectionBuilder.Default
//                 .AddEndpoint("amqp://test2:test2@localhost:5672/")
//                 .ConnectAsync();
//
//             var publisher = connection.CreatePublisher("amq.fanout",
//                 builder => builder
//                     .UsePlainTextSerializer()
//                     .PublisherConfirms()
//                 );
//
//             var sw = Stopwatch.StartNew();
//
//             await publisher.PublishAsync(BuildDummyText(1048576));
//
//             for (var i = 0; i < 10; i++)
//             {
//                 await publisher.PublishAsync(BuildDummyText(1048576));
//             }
//
//             await publisher.DisposeAsync();
//
//             sw.Stop();
//
//             this.output.WriteLine(sw.ElapsedMilliseconds.ToString());
//         }
//
//         [Fact]
//         public async Task TestConsumer()
//         {
//             //var connection = new Connection(ConnectionString.Create("amqp://rpeesesf:naQF5gZbGA9GzNHkSKE4QxwBt__Lsmu-@beaver.rmq.cloudamqp.com/rpeesesf"));
//             await using var connection = await ConnectionBuilder.Default
//                 .AddEndpoint("amqp://test1:test1@localhost:5672/")
//                 .UseDefaults()
//                 .ConnectAsync();
//
//             var cancelation = new CancellationTokenSource();
//             var num = 0;
//             var consumer = connection.Consumer(
//                 builder => builder
//                     .BindToQueue("test-queue")
//                     .EachMessageAcknowledgement()
//                     .PrefetchCount(10)
//                     .UsePlainTextSerializer()
//                     .AddMessageHandler((message, properties, content) =>
//                     {
//                         num++;
//
//                         if (num == 15)
//                         {
//                             cancelation.Cancel();
//                         }
//
//                         var body = content.GetContent<string>();
//
//                         return new ValueTask<bool>(true);
//                     })
//             );
//
//             await consumer.ConsumeAsync(cancelation.Token);
//         }
//
//         [Header("test", "wokrs")]
//         public class DummyClass
//         {
//
//         }
//
//         private const string TextFragment = "Lorem ipsum dolor sit amet, ne putent ornatus expetendis vix. Ea sed suas accusamus. Possim prodesset maiestatis sea te, graeci tractatos evertitur ad vix, sit an sale regione facilisi. Vel cu suscipit perfecto voluptaria. Diam soleat eos ex, his liber causae saperet et.";
//
//         public static string BuildDummyText(int length)
//         {
//             var builder = new StringBuilder(length);
//
//             while (builder.Length < length)
//             {
//                 builder.Append(TextFragment);
//             }
//
//             return builder.ToString(0, length);
//         }
//     }
// }