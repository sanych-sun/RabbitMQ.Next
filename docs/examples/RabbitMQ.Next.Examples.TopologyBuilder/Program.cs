using System;
using System.Threading.Tasks;
using RabbitMQ.Next.TopologyBuilder;

using RabbitMQ.Next.Abstractions;

namespace RabbitMQ.Next.Examples.TopologyBuilder
{
    class Program
    {
        static async Task Main()
        {
            try
            {
                Console.WriteLine("Hello World! This is topology builder based on RabbitMQ.Next library.");

                var connection = await ConnectionBuilder.Default
                    .Endpoint("amqp://test:pass@localhost:5672/")
                    .ConnectAsync();

                Console.WriteLine("Connection opened");

                await connection.ExchangeDeclareAsync("my-exchange", ExchangeType.Topic);
                Console.WriteLine("'my-exchange' was created with using library defaults (durable by default)");

                await connection.ExchangeDeclareAsync("my-advanced-exchange", ExchangeType.Topic,
                    builder => builder
                        .Transient());
                Console.WriteLine("'my-advanced-exchange' was created by explicitly configuring to be transient (non-durable)");

                Console.WriteLine("--------------------------------------------------------------");

                await connection.QueueDeclareAsync("my-queue");
                Console.WriteLine("'my-queue' was created with using library defaults (durable by default)");

                await connection.QueueDeclareAsync("my-advanced-queue",
                    builder => builder
                        .AutoDelete()
                        .WithMaxLength(1000));
                Console.WriteLine("'my-advanced-queue' was created by explicitly configuring to be auto-delete and max-length 1000");

                Console.WriteLine("--------------------------------------------------------------");

                await connection.QueueBindAsync("my-queue", "my-exchange",
                    builder => builder
                        .RoutingKey("cat")
                        .RoutingKey("dog"));
                Console.WriteLine("my-queue was bound to my-exchange by 2 bindings.");

                await connection.DisposeAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}