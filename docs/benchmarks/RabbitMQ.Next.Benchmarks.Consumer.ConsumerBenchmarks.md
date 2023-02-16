# Message consumer benchmarks

## Results

``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 22.04
Intel Core i7-8550U CPU 1.80GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.102
  [Host]     : .NET 6.0.13 (6.0.1322.58009), X64 RyuJIT
  Job-KYRXLO : .NET 6.0.13 (6.0.1322.58009), X64 RyuJIT

IterationCount=10  RunStrategy=Monitoring  WarmupCount=5  

```
|             Method |      Mean | Ratio |     Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------- |----------:|------:|----------:|------:|------:|----------:|
| ConsumeBaseLibrary |  76.05 ms |  1.00 | 1000.0000 |     - |     - |      4 MB |
|       ConsumeAsync | 121.48 ms |  1.60 |         - |     - |     - |      2 MB |

## Legend

Each test consume 1000 messages, with 1kB payload.

**ConsumeBaseLibrary** - consume using the official RabbitMQ-client, baseline benchmark.

**ConsumeAsync** - consume via RabbitMQ.Next library.