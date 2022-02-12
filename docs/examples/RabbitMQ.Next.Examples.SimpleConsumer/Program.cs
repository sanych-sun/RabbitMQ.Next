using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Consumer;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Serialization.PlainText;

namespace RabbitMQ.Next.Examples.SimpleConsumer
{
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Hello World! This is consumer based on RabbitMQ.Next library.");

            var connection = await ConnectionBuilder.Default
                .Endpoint("amqp://test:pass@localhost:5672/")
                .ConnectAsync();

            Console.WriteLine("Connection opened");

            var consumer = connection.Consumer(
                builder => builder
                    .BindToQueue("test-queue")
                    .UsePlainTextSerializer()
                    .MessageHandler((message, content) =>
                    {
                        Console.WriteLine($"[{DateTimeOffset.Now.TimeOfDay}] Message received via '{message.Exchange}' exchange: {content.GetContent<string>()}");
                        return true;
                    }));

            Console.WriteLine("Consumer created. Press Ctrl+C to exit.");

            var cancellation = new CancellationTokenSource();

            var consumeTask = consumer.ConsumeAsync(cancellation.Token);
            while (true)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.C && key.Modifiers == ConsoleModifiers.Control)
                {
                    cancellation.Cancel();
                    break;
                }
            }

            await consumeTask;
        }
    }
}