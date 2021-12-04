using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace RabbitMQ.Next.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args,
                DefaultConfig.Instance
                    .AddDiagnoser(MemoryDiagnoser.Default)
                    .AddJob(new Job
                    {
                        Run =
                        {
                            RunStrategy = RunStrategy.Monitoring, IterationCount = 20, WarmupCount = 5,
                        }
                    })
                    .AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByCategory)
            );
        }
    }
}