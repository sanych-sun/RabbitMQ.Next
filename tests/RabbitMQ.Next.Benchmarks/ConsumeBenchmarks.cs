// namespace RabbitMQ.Next.Benchmarks
// {
//     public class ConsumeBenchmarks
//     {
//         [Benchmark(Baseline = true)]
//         [BenchmarkCategory("Consume")]
//         public void ConsumeBaseLibrary()
//         {
//             var model = this.theirConnection.CreateModel();
//             model.BasicQos(0, 10, false);
//
//             var num = 0;
//             var consumer = new EventingBasicConsumer(model);
//             consumer.Received += (ch, ea) =>
//             {
//                 var body = ea.Body.ToArray();
//                 var message = Encoding.UTF8.GetString(body);
//                 num++;
//
//                 model.BasicAck(ea.DeliveryTag, false);
//             };
//             var tag = model.BasicConsume(queue: "test-queue",
//                 autoAck: false,
//                 consumer: consumer);
//
//             while (num <= messagesCount)
//             {
//                 Thread.Sleep(10);
//             }
//
//             model.BasicCancel(tag);
//             model.Close();
//         }
//
//         [Benchmark]
//         [BenchmarkCategory("Consume")]
//         public async Task ConsumeAsync()
//         {
//             var cancellation = new CancellationTokenSource();
//             var num = 0;
//             var consumer = this.connection.Consumer(
//                 b => b
//                     .BindToQueue("test-queue")
//                     .PrefetchCount(10)
//                     .EachMessageAcknowledgement()
//                     .UseFormatter(new StringTypeFormatter())
//                     .AddMessageHandler((message, properties, content) =>
//                     {
//                         var data = content.GetContent<string>();
//                         num++;
//                         if (num >= messagesCount)
//                         {
//                             if (!cancellation.IsCancellationRequested)
//                             {
//                                 cancellation.Cancel();
//                             }
//                         }
//
//                         return new ValueTask<bool>(true);
//                     }));
//
//             await consumer.ConsumeAsync(cancellation.Token);
//         }
//     }
// }