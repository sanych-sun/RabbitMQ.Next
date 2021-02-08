using System.Threading.Tasks;
using RabbitMQ.Next.MessagePublisher;
using RabbitMQ.Next.Transport;
using Xunit;

namespace RabbitMQ.Next.Tests
{
    // public class UnitTest1
    // {
    //     [Fact]
    //     public async Task Test1()
    //     {
    //         var connection = new Connection(ConnectionString.Create("amqp://test2:test2@localhost:5672/"));
    //
    //         await connection.ConnectAsync();
    //
    //         var publisher = connection.PublisherChannel(new PublisherChannelOptions {Exchange = "amq.fanout", localQueueLimit = 5}, new StringSerializer());
    //
    //         for (var i = 0; i < 1000; i++)
    //         {
    //             await publisher.WriteAsync( $"hello world {i}!", string.Empty);
    //         }
    //
    //         await publisher.CompleteAsync();
    //
    //         await connection.CloseAsync();
    //     }
    // }
}