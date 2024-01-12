using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Examples.DemoSaslAuthMechanism;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("Hello World! Will try to connect RabbitMQ server with RABBIT-CR-DEMO auth mechanism.");

        var connection = await ConnectionBuilder.Default
            .Endpoint("amqp://localhost:5672/")
            .WithRabbitCrDemoAuth("guest", "guest")
            .ConnectAsync()
            .ConfigureAwait(false);

        Console.WriteLine("Connection opened");
        Console.WriteLine("Press any key to close the connection");

        Console.ReadKey();
    }
}