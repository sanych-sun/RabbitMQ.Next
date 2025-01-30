using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next;
using RabbitMQ.Next.Consumer;
using RabbitMQ.Next.Serialization.PlainText;


Console.WriteLine("Hello World! This is consumer based on RabbitMQ.Next library.");

await using var connection = ConnectionBuilder.Default
    .UseConnectionString("amqp://guest:guest@localhost:5672/")
    .UsePlainTextSerializer()
    .Build();

await using var consumer = connection.Consumer(
builder => builder
        .BindToQueue("my-queue")
        .PrefetchCount(10));

Console.WriteLine("Consumer created. Press Ctrl+C to exit.");

using var cancellation = new CancellationTokenSource();

MonitorKeypressAsync(cancellation);

await consumer.ConsumeAsync((message, content) =>
{
    Console.WriteLine($"[{DateTimeOffset.Now.TimeOfDay}] Message received via '{message.Exchange}' exchange: {content.Get<string>()}");
} ,cancellation.Token);


static Task MonitorKeypressAsync(CancellationTokenSource cancellation)
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
