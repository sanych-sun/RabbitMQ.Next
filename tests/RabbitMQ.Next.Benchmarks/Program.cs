using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using RabbitMQ.Next.Benchmarks.PublishTests;

namespace RabbitMQ.Next.Benchmarks
{
    class Program
    {
        static async Task Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args,
                DefaultConfig.Instance
                    .AddDiagnoser(MemoryDiagnoser.Default)
                    .AddJob(new Job
                    {
                        Run =
                        {
                            RunStrategy = RunStrategy.Monitoring, IterationCount = 5, WarmupCount = 2,
                        }
                    })
                    .AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByCategory)
            );

            // var bc = new PublisherConfirmsBenchmarks();
            // await bc.Setup();
            // await Task.Yield();
            //
            // foreach (var tc in PublisherConfirmsBenchmarks.TestCases())
            // {
            //     await bc.PublishAsync(tc);
            //     await bc.PublishAsync(tc);
            // }
            //
            // Console.WriteLine("Done!");
        }
    }
}