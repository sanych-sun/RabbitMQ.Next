using System;
using System.Threading.Tasks;

using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Serialization.PlainText;

namespace RabbitMQ.Next.Examples.SimplePublisher
{
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Hello World! This is publisher based on RabbitMQ.Next library.");

            var connection = await ConnectionBuilder.Default
                .Endpoint("amqp://test:pass@localhost:5672/")
                .ConnectAsync();

            Console.WriteLine("Connection opened");

            var publisher = connection.Publisher("amq.fanout",
                builder => builder
                    .PublisherConfirms()
                    .UsePlainTextSerializer());

            Console.WriteLine("Publisher created. Type any text to send it to the 'amq.fanout' exchange. Enter empty string to exit");

            string input;
            while(true)
            {
                input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    break;
                }

                await publisher.PublishAsync(input);
            }
        }
    }
}