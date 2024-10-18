using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Next.Publisher;

namespace RabbitMQ.Next.Examples.DependencyInjection;

public class Worker : BackgroundService
{
    private readonly IPublisher _publisher;
    private readonly IHostLifetime _hostLifetime;

    public Worker(IConnection connection, IHostLifetime hostLifetime)
    {
        this._hostLifetime = hostLifetime;
        this._publisher = connection.Publisher("amq.fanout");
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await this._publisher.PublishAsync("Hello World!", cancellation: stoppingToken);
        Console.WriteLine("Message was published.");
        Console.WriteLine("Press [Enter] key to exit...");
        Console.ReadLine();
        await this._hostLifetime.StopAsync(stoppingToken);
    }
}
