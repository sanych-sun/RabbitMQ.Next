using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Consumer;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization.Formatters;
using IConnection = RabbitMQ.Next.Abstractions.IConnection;

namespace RabbitMQ.Next.Benchmarks
{

    [MemoryDiagnoser]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    public class PublishBenchmarks
    {
        private const int messagesCount = 10_000;

        private IConnection connection;
        private RabbitMQ.Client.IConnection theirConnection;

        private IReadOnlyList<string> messages;
        private IReadOnlyList<string> corrIds;

        public PublishBenchmarks()
        {
            var msg = new string[messagesCount];
            var corrIds = new string[messagesCount];
            for (var i = 0; i < messagesCount; i++)
            {
                msg[i] = $"{i} Lorem ipsum dolor sit amet, ne putent ornatus expetendis vix. Ea sed suas accusamus. Possim prodesset maiestatis sea te, graeci tractatos evertitur ad vix, sit an sale regione facilisi. Vel cu suscipit perfecto voluptaria. Diam soleat eos ex, his liber causae saperet et. Ne ipsum congue graecis sed"
                    + "{i} Lorem ipsum dolor sit amet, ne putent ornatus expetendis vix. Ea sed suas accusamus. Possim prodesset maiestatis sea te, graeci tractatos evertitur ad vix, sit an sale regione facilisi. Vel cu suscipit perfecto voluptaria. Diam soleat eos ex, his liber causae saperet et. Ne ipsum congue graecis sed"
                    + "{i} Lorem ipsum dolor sit amet, ne putent ornatus expetendis vix. Ea sed suas accusamus. Possim prodesset maiestatis sea te, graeci tractatos evertitur ad vix, sit an sale regione facilisi. Vel cu suscipit perfecto voluptaria. Diam soleat eos ex, his liber causae saperet et. Ne ipsum congue graecis sed"
                    + "{i} Lorem ipsum dolor sit amet, ne putent ornatus expetendis vix. Ea sed suas accusamus. Possim prodesset maiestatis sea te, graeci tractatos evertitur ad vix, sit an sale regione facilisi. Vel cu suscipit perfecto voluptaria. Diam soleat eos ex, his liber causae saperet et. Ne ipsum congue graecis sed";
                corrIds[i] = Guid.NewGuid().ToString();
            }

            this.corrIds = corrIds;
            this.messages = msg;
        }

        [GlobalSetup]
        public async Task Setup()
        {
            this.connection = await ConnectionBuilder.Default
                .AddEndpoint("amqp://test2:test2@localhost:5672/")
                .ConnectAsync();




            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://test2:test2@localhost:5672/");

            this.theirConnection = factory.CreateConnection();
        }

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("Publish")]
        public void PublishBaseLibrary()
        {
            var model = this.theirConnection.CreateModel();
            model.ConfirmSelect();

            for (var i = 0; i < this.messages.Count; i++)
            {
                var props = model.CreateBasicProperties();
                props.CorrelationId = this.corrIds[i];
                model.BasicPublish("amq.topic", "", props, Encoding.UTF8.GetBytes(this.messages[i]));
                model.WaitForConfirms();
            }
        }

        [Benchmark]
        [BenchmarkCategory("Publish")]
        public async Task PublishParallelAsync()
        {
            var publisher = await this.connection.CreatePublisherAsync("amq.topic",
                builder => builder
                    .PublisherConfirms()
                    .UseFormatter(new StringTypeFormatter()));

            await Task.WhenAll(Enumerable.Range(0, 10)
                .Select(async num =>
                {
                    await Task.Yield();

                    for (int i = num; i < this.messages.Count; i = i + 10)
                    {
                        await publisher.PublishAsync(this.corrIds[i], this.messages[i],
                            (state, message) => message.RoutingKey(state));

                    }
                })
                .ToArray());

            await publisher.DisposeAsync();
        }

        [Benchmark]
        [BenchmarkCategory("Publish")]
        public async Task PublishAsync()
        {
            var publisher = await this.connection.CreatePublisherAsync("amq.topic",
                builder => builder
                    .PublisherConfirms()
                    .UseFormatter(new StringTypeFormatter()));

            for (int i = 0; i < this.messages.Count; i++)
            {
                await publisher.PublishAsync(this.corrIds[i], this.messages[i],
                    (state, message) => message.CorrelationId(state));
            }

            await publisher.DisposeAsync();
        }

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("Consume")]
        public void ConsumeBaseLibrary()
        {
            var model = this.theirConnection.CreateModel();
            model.BasicQos(0, 10, false);

            var num = 0;
            var consumer = new EventingBasicConsumer(model);
            consumer.Received += (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                num++;

                model.BasicAck(ea.DeliveryTag, false);
            };
            var tag = model.BasicConsume(queue: "test-queue",
                autoAck: false,
                consumer: consumer);

            while (num <= messagesCount)
            {
                Thread.Sleep(10);
            }

            model.BasicCancel(tag);
            model.Close();
        }

        [Benchmark]
        [BenchmarkCategory("Consume")]
        public async Task ConsumeAsync()
        {
            var cancellation = new CancellationTokenSource();
            var num = 0;
            var consumer = this.connection.Consumer(
                b => b
                    .BindToQueue("test-queue")
                    .PrefetchCount(10)
                    .EachMessageAcknowledgement()
                    .UseFormatter(new StringTypeFormatter())
                    .AddMessageHandler((message, properties, content) =>
                    {
                        var data = content.GetContent<string>();
                        num++;
                        if (num >= messagesCount)
                        {
                            if (!cancellation.IsCancellationRequested)
                            {
                                cancellation.Cancel();
                            }
                        }

                        return new ValueTask<bool>(true);
                    }));

            await consumer.ConsumeAsync(cancellation.Token);
        }
    }
}