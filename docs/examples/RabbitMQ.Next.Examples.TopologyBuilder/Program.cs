using System;
using RabbitMQ.Next;
using RabbitMQ.Next.TopologyBuilder;

Console.WriteLine("Hello World! This is topology builder based on RabbitMQ.Next library.");

var connection = ConnectionBuilder.Default
    .UseConnectionString("amqp://guest:guest@localhost:5672/")
    .Build();

await connection.ConfigureAsync(async topology =>
{
    await topology.Exchange.DeclareAsync("my-exchange", ExchangeType.Topic);
    Console.WriteLine("'my-exchange' was created with using library defaults (durable by default)");

    await topology.Exchange.DeclareAsync("my-advanced-exchange", ExchangeType.Topic,
        builder => builder
            .AutoDelete());
    Console.WriteLine("'my-advanced-exchange' was created by explicitly configuring to be auto-delete");

    Console.WriteLine("--------------------------------------------------------------");

    await topology.Queue.DeclareQuorumAsync("my-queue");
    Console.WriteLine("Declare quorum queue named 'my-queue'");

    await topology.Queue.DeclareClassicAsync("my-advanced-queue",
        builder => builder
            .AutoDelete()
            .MaxLength(1000));
    Console.WriteLine("'my-advanced-queue' was created by explicitly configuring to be auto-delete and max-length 1000");

    Console.WriteLine("--------------------------------------------------------------");

    await topology.Queue.BindAsync("my-queue", "amq.fanout");
    await topology.Queue.BindAsync("my-queue", "my-exchange", "cat");
    await topology.Queue.BindAsync("my-queue", "my-exchange", "dog");
    Console.WriteLine("my-queue was bound to my-exchange by 2 bindings.");
});
