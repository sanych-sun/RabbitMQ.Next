using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

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
                            RunStrategy = RunStrategy.Monitoring, IterationCount = 5, WarmupCount = 1,
                        }
                    })
                    .AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByCategory)
            );

            // var bc = new PublishBenchmarks();
            // await bc.Setup();
            //
            // await Task.Yield();
            // await Test(bc);
        }

        static Task Test(PublishBenchmarks bc)
        {
            return bc.PublishAsync();
        }
    }
}