using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RabbitMQ.Client.Events;
using RabbitMQ.Next.Consumer;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Serialization.PlainText;
using RabbitMQ.Next.TopologyBuilder;
using ConnectionFactory = RabbitMQ.Client.ConnectionFactory;

namespace RabbitMQ.Next.Benchmarks.Consumer;

public class ConsumerBenchmarks
{
    private readonly int messagesCount = 1000;
    private readonly string queueName = "test-queue";
    private IConnection connection;
    private RabbitMQ.Client.IConnection theirConnection;


    [GlobalSetup]
    public async Task Setup()
    {
        this.connection = await ConnectionBuilder.Default
            .Endpoint(Helper.RabbitMqConnection)
            .ConnectAsync();

        ConnectionFactory factory = new ConnectionFactory()
        {
            Uri = Helper.RabbitMqConnection,
            DispatchConsumersAsync = true,
        };
        this.theirConnection = factory.CreateConnection();

        await this.connection.ConfigureAsync(async topology =>
        {
            await topology.Queue.DeclareClassicAsync(this.queueName);
            await topology.Queue.BindAsync(this.queueName, "amq.fanout");
            await topology.Queue.PurgeAsync(this.queueName);
        });
        
        var publisher = this.connection.Publisher("amq.fanout", builder => builder.UsePlainTextSerializer());
            
        var payload = Helper.BuildDummyText(1024);
            
        for (int i = 0; i < this.messagesCount * 20; i++) // 15 runs for benchmark
        {
            await publisher.PublishAsync(payload,
                message => message
                    .SetMessageId(Guid.NewGuid().ToString())
                    .SetCorrelationId(Guid.NewGuid().ToString())
                    .SetApplicationId("testApp"));
        }
            
        await publisher.DisposeAsync();
        Console.WriteLine("Publisher - done");
    }
        
        
    [Benchmark(Baseline = true)]
    public void ConsumeBaseLibrary()
    {
        var model = this.theirConnection.CreateModel();

        var num = 0;
        var consumer = new AsyncEventingBasicConsumer(model);
        
        var manualResetEvent = new ManualResetEvent(false);
        
        consumer.Received += (ch, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var messageId = ea.BasicProperties.MessageId;
            
            num++;
            model.BasicAck(ea.DeliveryTag, false);
        
            if (num >= this.messagesCount)
            {
                manualResetEvent.Set();
            }

            return Task.CompletedTask;
        };
        
        model.BasicQos(0, 10, false);
        
        var tag = model.BasicConsume(
            queue: this.queueName,
            autoAck: false,
            consumer: consumer,
            consumerTag: string.Empty,
            noLocal: false,
            exclusive: false,
            arguments: null);
        
        manualResetEvent.WaitOne();
        model.BasicCancel(tag);
        model.Close();
        
        Console.WriteLine($"Consumed: {num}");
    }

    [Benchmark]
    public async Task ConsumeAsync()
    {
        var num = 0;
        var cs = new CancellationTokenSource();
        var consumer = this.connection.Consumer(
            b => b
                .BindToQueue(this.queueName)
                .PrefetchCount(10)
                .UsePlainTextSerializer());

        var consumeTask = consumer.ConsumeAsync(message =>
        {
            var data = message.Content<string>();
            var messageId = message.Properties.MessageId;
            num++;
            if (num >= this.messagesCount)
            {
                cs.Cancel();
            }

            return default;
        },cs.Token);
        await consumeTask;

        Console.WriteLine($"Consumed: {num}");
    }
    
    // [Benchmark]
    // public async Task ConsumeParallelAsync()
    // {
    //     var num = 0;
    //     var cs = new CancellationTokenSource();
    //     var consumer = this.connection.Consumer(
    //         b => b
    //             .BindToQueue(this.queueName)
    //             .PrefetchCount(10)
    //             .ConcurrencyLevel(5)
    //             .UsePlainTextSerializer());
    //
    //     var consumeTask = consumer.ConsumeAsync(message =>
    //     {
    //         var data = message.Content<string>();
    //         var messageId = message.Properties.MessageId;
    //
    //         Interlocked.Increment(ref num);
    //         if (num >= this.messagesCount)
    //         {
    //             cs.Cancel();
    //         }
    //         
    //         return default;
    //     },cs.Token);
    //     await consumeTask;
    //
    //     Console.WriteLine($"Consumed: {num}");
    // }
}