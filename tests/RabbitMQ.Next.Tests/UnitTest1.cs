using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Consumer;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Attributes;
using RabbitMQ.Next.Serialization.PlainText;
using Xunit;
using Xunit.Abstractions;

namespace RabbitMQ.Next.Tests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper output;

        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
        }

        // [Fact]
        // public async Task Test1()
        // {
        //     await using var connection = ConnectionBuilder.Default
        //         .UseConnectionString("amqp://guest:guest@localhost:5672/")
        //         .UsePlainTextSerializer()
        //         .Build();
        //
        //     var publisher = connection.Publisher("amq.fanout");
        //
        //     var sw = Stopwatch.StartNew();
        //
        //     for (var i = 0; i < 100; i++)
        //     {
        //         await publisher.PublishAsync($"test{i}");
        //     }
        //
        //     await publisher.DisposeAsync();
        //
        //     sw.Stop();
        //
        //     this.output.WriteLine(sw.ElapsedMilliseconds.ToString());
        // }

        // [Fact]
        // public async Task TestConsumer()
        // {
        //     //var connection = new Connection(ConnectionString.Create("amqp://rpeesesf:naQF5gZbGA9GzNHkSKE4QxwBt__Lsmu-@beaver.rmq.cloudamqp.com/rpeesesf"));
        //     await using var connection = ConnectionBuilder.Default
        //         .UseConnectionString("amqp://test1:test1@localhost:5672/")
        //         .UsePlainTextSerializer()
        //         .Build();
        //
        //     var num = 0;
        //     var tcs = new TaskCompletionSource();
        //     var consumer = connection.Consumer(
        //         builder => builder
        //             .BindToQueue("my-queue")
        //             .PrefetchCount(10));
        //
        //     var comsumeTask = consumer.ConsumeAsync((message, content) =>
        //     {
        //         num++;
        //
        //         if (num == 100)
        //         {
        //             tcs.SetResult();
        //         }
        //
        //         var body = content.Get<string>();
        //     }, CancellationToken.None);
        //
        //     await tcs.Task;
        //     await consumer.DisposeAsync();
        //     await comsumeTask;
        // }
        //
        // [Header("test", "wokrs")]
        // public class DummyClass
        // {
        //
        // }
        //
        // private const string TextFragment = "Lorem ipsum dolor sit amet, ne putent ornatus expetendis vix. Ea sed suas accusamus. Possim prodesset maiestatis sea te, graeci tractatos evertitur ad vix, sit an sale regione facilisi. Vel cu suscipit perfecto voluptaria. Diam soleat eos ex, his liber causae saperet et.";
        //
        // public static string BuildDummyText(int length)
        // {
        //     var builder = new StringBuilder(length);
        //
        //     while (builder.Length < length)
        //     {
        //         builder.Append(TextFragment);
        //     }
        //
        //     return builder.ToString(0, length);
        // }
    }
}
