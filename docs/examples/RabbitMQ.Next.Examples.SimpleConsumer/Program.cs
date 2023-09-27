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
            .Endpoint("amqp://test2:test2@localhost:5672/")
            .ConnectAsync()
            .ConfigureAwait(false);

        Console.WriteLine("Connection opened");

        await using var consumer = connection.Consumer(
            builder => builder
                .BindToQueue("test-queue")
                .PrefetchCount(10)
                .UsePlainTextSerializer());

        Console.WriteLine("Consumer created. Press Ctrl+C to exit.");

        var cancellation = new CancellationTokenSource();

        MonitorKeypressAsync(cancellation);

        await consumer.ConsumeAsync(async message =>
        {
            Console.WriteLine($"[{DateTimeOffset.Now.TimeOfDay}] Message received via '{message.Exchange}' exchange: {message.Content<string>()}");
        } ,cancellation.Token).ConfigureAwait(false);
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
