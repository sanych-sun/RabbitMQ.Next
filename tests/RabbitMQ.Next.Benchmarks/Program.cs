using System.Globalization;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Perfolizer.Horology;

namespace RabbitMQ.Next.Benchmarks;

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
                        RunStrategy = RunStrategy.Monitoring, IterationCount = 10, WarmupCount = 5,
                    }
                })
                .WithSummaryStyle(new SummaryStyle(CultureInfo.InvariantCulture, true, SizeUnit.KB, TimeUnit.Millisecond, false, true))
                .AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByCategory)
        );
    }
}