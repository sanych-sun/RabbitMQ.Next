using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RabbitMQ.Client;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Consumer;
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
        private const int messagesCount = 100_000;

        private IConnection connection;
        private IModel model;
        private IPublisher publisher;
        private IReadOnlyList<string> messages;
        private IReadOnlyList<string> corrIds;

        public PublishBenchmarks()
        {
            var msg = new string[messagesCount];
            var corrIds = new string[messagesCount];
            for (var i = 0; i < messagesCount; i++)
            {
                msg[i] = $"simple text message that has to be encoded and send to rmq {i}";
                corrIds[i] = Guid.NewGuid().ToString();
            }

            this.corrIds = corrIds;
            this.messages = msg;
        }

        [GlobalSetup]
        public async Task Setup()
        {
            this.connection = await ConnectionBuilder.Default()
                .AddEndpoint("amqp://test2:test2@localhost:5672/")
                .ConnectAsync();

            this.publisher = this.connection.Publisher("amq.topic",
                builder => builder
                    .UseFormatter(new StringTypeFormatter()));

            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://test2:test2@localhost:5672/");

            this.model = factory.CreateConnection().CreateModel();
        }

        [Benchmark(Baseline = true)]
        public void Publish()
        {
            for (var i = 0; i < this.messages.Count; i++)
            {
                var props = this.model.CreateBasicProperties();
                props.CorrelationId = this.corrIds[i];
                this.model.BasicPublish("amq.topic", "", props, Encoding.UTF8.GetBytes(this.messages[i]));
            }
        }

        [Benchmark]
        public async Task PublishAsync()
        {
            for (int i = 0; i < this.messages.Count; i++)
            {
                await this.publisher.PublishAsync(this.messages[i], properties: new MessageProperties { CorrelationId = this.corrIds[i]});
            }

            // var processed = 0;
            // var consumerCancellation = new CancellationTokenSource();
            //
            // var consumer = connection.Consumer(
            //      builder => builder
            //          .PrefetchCount(200)
            //          .UseFormatter(new ArrayTypeFormatter())
            //          //.MultipleMessageAcknowledgement(TimeSpan.FromSeconds(5), 100)
            //          .AddMessageHandler((message, props, content) =>
            //          {
            //              var data = content.GetContent<byte[]>();
            //
            //              processed++;
            //
            //              if (processed == messagesCount)
            //              {
            //                  consumerCancellation.Cancel();
            //              }
            //
            //              return new ValueTask<bool>(true);
            //          })
            //          .BindToQueue("test-queue"));
            //
            // var consumerTask = consumer.ConsumeAsync(consumerCancellation.Token);
            //
            //
            //
            // await consumerTask;
        }
    }
}