using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Consumer;
using RabbitMQ.Next.Serialization.PlainText;

namespace RabbitMQ.Next.Examples.SimpleConsumer;

internal static class Program
{
    private static async Task Main()
    {
        Console.WriteLine("Hello World! This is consumer based on RabbitMQ.Next library.");

        var connection = ConnectionBuilder.Default
            .Endpoint("amqp://guest:guest@localhost:5672/")
            .UsePlainTextSerializer()
            .Build();

        Console.WriteLine("Connection opened");

        await using var consumer = connection.Consumer(
            builder => builder
                .BindToQueue("my-queue")
                .PrefetchCount(10));

        Console.WriteLine("Consumer created. Press Ctrl+C to exit.");

        var cancellation = new CancellationTokenSource();

        MonitorKeypressAsync(cancellation);

        await consumer.ConsumeAsync((message, content) =>
        {
            Console.WriteLine($"[{DateTimeOffset.Now.TimeOfDay}] Message received via '{message.Exchange}' exchange: {content.Get<string>()}");
        } ,cancellation.Token);
    }
    
    private static Task MonitorKeypressAsync(CancellationTokenSource cancellation)
    {
        void WaitForInput()
        {
            ConsoleKeyInfo key;
            do 
            {
                key = Console.ReadKey(true);

            } while (key.Key != ConsoleKey.C && key.Modifiers != ConsoleModifiers.Control);

            if (!cancellation.IsCancellationRequested)
            {
                cancellation.Cancel();
            }
        }

        return Task.Run(WaitForInput);
    }
}
