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

namespace RabbitMQ.Next.Benchmarks.Consumer
{
    public class ConsumerBenchmarks
    {
        private readonly int messagesCount = 10000;
        private readonly string queueName = "test-queue";
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

            await this.connection.QueueDeclareAsync(this.queueName);
            await this.connection.QueueBindAsync(this.queueName, "amq.fanout");
            await this.connection.QueuePurgeAsync(this.queueName);

            var publisher = this.connection.Publisher("amq.fanout",
                builder => builder.UsePlainTextSerializer());

            var payload = Helper.BuildDummyText(10240);

            for (int i = 0; i < this.messagesCount * 30; i++) // 25 runs for benchmark
            {
                await publisher.PublishAsync(payload,
                    message => message
                        .MessageId(Guid.NewGuid().ToString())
                        .CorrelationId(Guid.NewGuid().ToString())
                        .ApplicationId("testApp"));
            }

            await publisher.DisposeAsync();
            Console.WriteLine("Publisher - done");
        }


        [Benchmark(Baseline = true)]
        public void ConsumeBaseLibrary()
        {
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

                var messageId = ea.BasicProperties.MessageId;

                model.BasicAck(ea.DeliveryTag, false);

                if (num >= this.messagesCount)
                {
                    manualResetEvent.Set();
                }
            };

            model.BasicQos(0, 10, true);

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
                    .UsePlainTextSerializer()
                    .MessageHandler((message, content) =>
                    {
                        var data = content.GetContent<string>();
                        var messageId = content.Properties.MessageId;
                        num++;
                        if (num >= this.messagesCount)
                        {
                            cs.Cancel();
                        }

                        return new ValueTask<bool>(true);
                    }));

            var consumeTask = consumer.ConsumeAsync(cs.Token);
            await consumeTask;

            Console.WriteLine($"Consumed: {num}");
        }
    }
}