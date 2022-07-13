using System;
using System.Threading.Tasks;

using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Serialization.PlainText;

namespace RabbitMQ.Next.Examples.SimplePublisher;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("Hello World! This is publisher based on RabbitMQ.Next library.");

        var connection = await ConnectionBuilder.Default
            .Endpoint("amqp://test:pass@localhost:5672/")
            .ConfigureSerialization(builder => builder
                .UsePlainTextSerializer())
            .ConnectAsync();

        Console.WriteLine("Connection opened");

        var publisher = connection.Publisher("amq.fanout");

        Console.WriteLine("Publisher created. Type any text to send it to the 'amq.fanout' exchange. Enter empty string to exit");

        string input;
        while(true)
        {
            input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
            {
                break;
            }

            try
            {
                await publisher.PublishAsync(input);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}