using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Next;
using RabbitMQ.Next.DependencyInjection;
using RabbitMQ.Next.Examples.DependencyInjection;
using RabbitMQ.Next.Serialization.PlainText;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
    services.AddRabbitMQConnection(
        builder => builder
            .UseConnectionString("amqp://guest:guest@localhost:5672/")
            .UsePlainTextSerializer());
    services.AddHostedService<Worker>();
});

using IHost host = builder.Build();

host.Run();
