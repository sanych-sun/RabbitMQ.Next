// using System.Diagnostics;
// using System.Threading.Tasks;
// using RabbitMQ.Next.MessagePublisher;
// using RabbitMQ.Next.MessagePublisher.Abstractions;
// using RabbitMQ.Next.MessagePublisher.Abstractions.Transformers;
// using RabbitMQ.Next.MessagePublisher.Transformers;
// using RabbitMQ.Next.Serialization;
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
//             //var connection = new Connection(ConnectionString.Create("amqp://rpeesesf:naQF5gZbGA9GzNHkSKE4QxwBt__Lsmu-@beaver.rmq.cloudamqp.com/rpeesesf"));
//             var connection = new Connection(ConnectionString.Create("amqp://test2:test2@localhost:5672/"));
//
//
//             await connection.ConnectAsync();
//
//             var serializer = new Serializer(new SingleFormatterSource(new StringFormatter()));
//
//             var publisher = connection.Publisher(serializer, new IMessageTransformer[]
//             {
//                 new ApplicationIdTransformer("unittest"),
//                 new ExchangeTransformer("amq.fanout"),
//             });
//
//             var sw = Stopwatch.StartNew();
//
//             //for (var i = 0; i < 10000; i++)
//             {
//                 await publisher.PublishAsync($"hello world 100500!", flags: PublishFlags.Mandatory);
//             }
//
//             await Task.Delay(10000);
//             await publisher.CompleteAsync();
//
//             sw.Stop();
//
//             this.output.WriteLine(sw.ElapsedMilliseconds.ToString());
//
//             await connection.CloseAsync();
//         }
//     }
// }