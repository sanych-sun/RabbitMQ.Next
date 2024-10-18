using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RabbitMQ.Client;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Serialization.PlainText;

namespace RabbitMQ.Next.Benchmarks.Publisher;

public class PublisherBenchmarks
{
    private readonly IConnection connection;
    private readonly RabbitMQ.Client.IConnection theirConnection;

    public PublisherBenchmarks()
    {
        ConnectionFactory factory = new ConnectionFactory
        {
            Uri = Helper.RabbitMqConnection,
        };
        this.theirConnection = factory.CreateConnection();
        
        this.connection = ConnectionBuilder.Default
            .UseConnectionString(Helper.RabbitMqConnection)
            .UsePlainTextSerializer()
            .Build();
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
            props.DeliveryMode = 2; // this is default for RabbitMQ.Next
            model.BasicPublish("amq.topic", "", props, Encoding.UTF8.GetBytes(data.Payload));
            model.WaitForConfirms();
        }

        model.Close();
    }

    [Benchmark]
    [ArgumentsSource(nameof(TestCases))]
    public async Task PublishParallelAsync(TestCaseParameters parameters)
    {
        var publisher = this.connection.Publisher("amq.topic");

        await Task.WhenAll(Enumerable.Range(0, 10)
            .Select(async num =>
            {
                await Task.Yield();

                for (int i = num; i < parameters.Messages.Count; i += 10)
                {
                    var data = parameters.Messages[i];
                    await publisher.PublishAsync(data, data.Payload,
                        (state, message) => message.SetCorrelationId(state.CorrelationId)).ConfigureAwait(false);
                }
            })
            .ToArray())
            .ConfigureAwait(false);

        await publisher.DisposeAsync().ConfigureAwait(false);
    }

    [Benchmark]
    [ArgumentsSource(nameof(TestCases))]
    public async Task PublishAsync(TestCaseParameters parameters)
    {
        var publisher = this.connection.Publisher("amq.topic");

        for (int i = 0; i < parameters.Messages.Count; i++)
        {
            var data = parameters.Messages[i];
            await publisher.PublishAsync(data, data.Payload,
                (state, message) => message.SetCorrelationId(state.CorrelationId)).ConfigureAwait(false);
        }

        await publisher.DisposeAsync().ConfigureAwait(false);
    }

    [GlobalCleanup]
    public async ValueTask CleanUpOfficialLibrary()
    {
        this.theirConnection.Close();
        this.theirConnection.Dispose();
        await this.connection.DisposeAsync();
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

        yield return GenerateTestCase(1024, 1_000, "1024 (1 kB)");
        yield return GenerateTestCase(10240, 1_000, "10240 (10 kB)");
        yield return GenerateTestCase(102400, 1_000, "102400 (100 kB)");
        yield return GenerateTestCase(204800, 1_000, "204800 (200 kB)");
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
