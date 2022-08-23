﻿using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Consumer;
using RabbitMQ.Next.Serialization.PlainText;

namespace RabbitMQ.Next.Examples.SimpleConsumer;

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
                .PrefetchCount(10)
                .UsePlainTextSerializer()
                .MessageHandler(message =>
                {
                    Console.WriteLine($"[{DateTimeOffset.Now.TimeOfDay}] Message received via '{message.Exchange}' exchange: {message.Content<string>()}");
                    return true;
                }));

        Console.WriteLine("Consumer created. Press Ctrl+C to exit.");

        var cancellation = new CancellationTokenSource();

        MonitorKeypressAsync(cancellation);

        await consumer.ConsumeAsync(cancellation.Token);
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
