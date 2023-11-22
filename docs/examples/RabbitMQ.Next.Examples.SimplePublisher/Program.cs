using System;
using System.Diagnostics;
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
            .Endpoint("amqp://guest:guest@localhost:5672/")
            .UsePlainTextSerializer()
            .ConnectAsync()
            .ConfigureAwait(false);

        Console.WriteLine("Connection opened");

        await using var publisher = connection.Publisher("amq.fanout", 
            builder => builder.UsePublishMiddleware(async (message, content, next) =>
            {
                var ts = Stopwatch.GetTimestamp();
                await next(message, content);
                Console.WriteLine(Stopwatch.GetTimestamp() - ts);
            }).UsePublishMiddleware((message, content) =>
            {
                message.SetType(message.ClrType.Name);
            }));

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
                await publisher.PublishAsync(input).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}