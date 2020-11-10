using System.Threading.Tasks;
using RabbitMQ.Next.MessagePublisher;
using RabbitMQ.Next.Messaging.Common.Serializers;
using RabbitMQ.Next.Transport;
using Xunit;

namespace RabbitMQ.Next.Tests
{
    public class UnitTest1
    {
        // [Fact]
        // public async Task Test1()
        // {
        //     var connection = new Connection(ConnectionString.Create("amqp://test2:test2@localhost:5672/"));
        //
        //     await connection.ConnectAsync();
        //
        //     var publisher = new MessagePublisher<string>(connection, new StringSerializer());
        //     await publisher.PublishAsync("my.fanout", "hello world!");
        //     await Task.Delay(10000);
        //
        //     await connection.CloseAsync();
        // }
    }
}