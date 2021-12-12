using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Diagnostics.Tracing.Parsers.FrameworkEventSource;
using RabbitMQ.Client.Events;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Consumer;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Serialization.PlainText;
using RabbitMQ.Next.TopologyBuilder;
using ConnectionFactory = RabbitMQ.Client.ConnectionFactory;

namespace RabbitMQ.Next.Benchmarks.Consumer
{
    public class ConsumerBenchmarks
    {
        private readonly int messagesCount = 10000;
        private string queueName;
        private IConnection connection;
        private RabbitMQ.Client.IConnection theirConnection;


        [GlobalSetup]
        public async Task Setup()
        {
            this.connection = await ConnectionBuilder.Default
                .Endpoint(Helper.RabbitMqConnection)
                .ConnectAsync();

            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = Helper.RabbitMqConnection;
            this.theirConnection = factory.CreateConnection();

            await this.connection.QueuePurgeAsync("test-queue");

            var publisher = this.connection.Publisher("amq.fanout",
                builder => builder.UsePlainTextSerializer());

            var payload = Helper.BuildDummyText(10240);

            for (int i = 0; i < this.messagesCount * 25; i++)
            {
                await publisher.PublishAsync(payload,
                    message => message
                        .MessageId(Guid.NewGuid().ToString())
                        .CorrelationId(Guid.NewGuid().ToString())
                        .ApplicationId("testApp"));
            }

            Console.WriteLine("Publisher - disposing");
            await publisher.DisposeAsync();
            Console.WriteLine("Publisher - disposed");
        }


        [Benchmark(Baseline = true)]
        public void ConsumeBaseLibrary()
        {
            Console.WriteLine("Start consuming: base lib........");

            var model = this.theirConnection.CreateModel();
            model.BasicQos(0, 10, false);

            var num = 0;
            var consumer = new EventingBasicConsumer(model);

            var manualResetEvent = new ManualResetEvent(false);

            consumer.Received += (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                num++;

                var mid = ea.BasicProperties.MessageId;

                model.BasicAck(ea.DeliveryTag, false);

                if (num >= this.messagesCount)
                {
                    manualResetEvent.Set();
                }
            };

            model.BasicQos(0, 10, true);

            var tag = model.BasicConsume(
                queue: "test-queue",
                autoAck: false,
                consumer: consumer,
                consumerTag: string.Empty,
                noLocal: true,
                exclusive: false,
                arguments: null);

            manualResetEvent.WaitOne();
            model.BasicCancel(tag);
            model.Close();
        }

        [Benchmark]
        public async Task ConsumeAsync()
        {
            var cancellation = new CancellationTokenSource();
            var num = 0;
            var tcs = new TaskCompletionSource();
            var consumer = this.connection.Consumer(
                b => b
                    .BindToQueue("test-queue")
                    .PrefetchCount(10)
                    .UsePlainTextSerializer()
                    .MessageHandler((message, content) =>
                    {
                        var data = content.GetContent<string>();
                        var messageId = content.Properties.MessageId;
                        num++;
                        if (num >= this.messagesCount)
                        {
                            tcs.TrySetResult();
                        }

                        return new ValueTask<bool>(true);
                    }));

            var consumeTask = consumer.ConsumeAsync(cancellation.Token);

            await tcs.Task;
            await consumer.DisposeAsync();
            await consumeTask;
        }
    }
}