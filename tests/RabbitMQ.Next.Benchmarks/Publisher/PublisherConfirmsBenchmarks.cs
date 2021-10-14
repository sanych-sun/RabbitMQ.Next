using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RabbitMQ.Client;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Serialization.PlainText;
using IConnection = RabbitMQ.Next.Abstractions.IConnection;

namespace RabbitMQ.Next.Benchmarks.Publisher
{
    public class PublisherConfirmsBenchmarks
    {
        private IConnection connection;
        private RabbitMQ.Client.IConnection theirConnection;

        [GlobalSetup]
        public async Task Setup()
        {
            this.connection = await ConnectionBuilder.Default
                .AddEndpoint(Helper.RabbitMqConnection)
                .ConnectAsync();

            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = Helper.RabbitMqConnection;

            this.theirConnection = factory.CreateConnection();
        }

        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(TestCases))]
        public void PublishBaseLibrary(TestCaseParameters parameters)
        {
            var model = this.theirConnection.CreateModel();
            model.ConfirmSelect();

            for (var i = 0; i < parameters.Messages.Count; i++)
            {
                var data = parameters.Messages[i];
                var props = model.CreateBasicProperties();
                props.CorrelationId = data.CorrelationId;
                model.BasicPublish("amq.topic", "", props, Encoding.UTF8.GetBytes(data.Payload));
                model.WaitForConfirms();
            }

            model.Close();
        }

        [Benchmark]
        [ArgumentsSource(nameof(TestCases))]
        public async Task PublishParallelAsync(TestCaseParameters parameters)
        {
            var publisher = await this.connection.CreatePublisherAsync("amq.topic",
                builder => builder
                    .PublisherConfirms()
                    .UsePlainTextSerializer()
                );

            await Task.WhenAll(Enumerable.Range(0, 10)
                .Select(async num =>
                {
                    await Task.Yield();

                    for (int i = num; i < parameters.Messages.Count; i = i + 10)
                    {
                        var data = parameters.Messages[i];
                        await publisher.PublishAsync(data, data.Payload,
                            (state, message) => message.CorrelationId(state.CorrelationId));

                    }
                })
                .ToArray());

            await publisher.DisposeAsync();
        }

        [Benchmark]
        [ArgumentsSource(nameof(TestCases))]
        public async Task PublishAsync(TestCaseParameters parameters)
        {
            var publisher = await this.connection.CreatePublisherAsync("amq.topic",
                builder => builder
                    .PublisherConfirms()
                    .UsePlainTextSerializer()
                );

            for (int i = 0; i < parameters.Messages.Count; i++)
            {
                var data = parameters.Messages[i];
                await publisher.PublishAsync(data, data.Payload,
                    (state, message) => message.CorrelationId(state.CorrelationId));
            }

            await publisher.DisposeAsync();
        }

        public static IEnumerable<TestCaseParameters> TestCases()
        {
            TestCaseParameters GenerateTestCase(int payloadLen, int count, string name)
            {
                var payload = Helper.BuildDummyText(payloadLen);
                var messages = new List<(string Payload, string CorrelationId)>(count);
                for (int i = 0; i < count; i++)
                {
                    messages.Add((payload, Guid.NewGuid().ToString()));
                }

                return new TestCaseParameters(name, messages);
            }

            yield return GenerateTestCase(128, 10_000, "10_000 (128 B)");
            yield return GenerateTestCase(1024, 10_000, "10_000 (1 kB)");
            yield return GenerateTestCase(10240, 10_000, "10_000 (10 kB)");
            yield return GenerateTestCase(102400, 10_000, "10_000 (100 kB)");
            yield return GenerateTestCase(1048576, 1_000, "1_000 (1 MB)");
        }

        public class TestCaseParameters
        {
            public TestCaseParameters(string name, IReadOnlyList<(string Payload, string CorrelationId)> messages)
            {
                this.Name = name;
                this.Messages = messages;
            }

            public IReadOnlyList<(string Payload, string CorrelationId)> Messages { get; }

            public string Name { get; }

            public override string ToString() => this.Name;
        }
    }
}