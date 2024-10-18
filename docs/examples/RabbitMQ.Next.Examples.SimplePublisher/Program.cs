using System;
using RabbitMQ.Next;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Serialization.PlainText;

Console.WriteLine("Hello World! This is publisher based on RabbitMQ.Next library.");

var connection = ConnectionBuilder.Default
    .UseConnectionString("amqp://guest:guest@localhost:5672/")
    .UsePlainTextSerializer()
    .Build();

await using var publisher = connection.Publisher("amq.fanout");

Console.WriteLine("Publisher created. Type any text to send it to the 'amq.fanout' exchange. Enter empty string to exit");

while(true)
{
    var input = Console.ReadLine();
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
