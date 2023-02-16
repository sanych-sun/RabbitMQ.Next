# Message publisher benchmarks

## Results

``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 22.04
Intel Core i7-8550U CPU 1.80GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.102
  [Host]     : .NET 6.0.13 (6.0.1322.58009), X64 RyuJIT
  Job-WYHKPC : .NET 6.0.13 (6.0.1322.58009), X64 RyuJIT

IterationCount=10  RunStrategy=Monitoring  WarmupCount=5  

```
|               Method |      parameters | Mean [ms] | Ratio |       Gen0 |       Gen1 |       Gen2 | Allocated [KB] |
|--------------------- |---------------- |----------:|------:|-----------:|-----------:|-----------:|---------------:|
|   **PublishBaseLibrary** |     **1024 (1 kB)** |    **155.76** |  **1.00** |          **-** |          **-** |          **-** |         **1496.7** |
| PublishParallelAsync |     1024 (1 kB) |     60.57 |  0.39 |          - |          - |          - |         437.28 |
|         PublishAsync |     1024 (1 kB) |    158.53 |  1.02 |          - |          - |          - |         580.95 |
|                      |                 |           |       |            |            |            |                |
|   **PublishBaseLibrary** |   **10240 (10 kB)** |    **168.25** |  **1.00** |  **2000.0000** |          **-** |          **-** |        **10496.7** |
| PublishParallelAsync |   10240 (10 kB) |     63.13 |  0.38 |          - |          - |          - |         439.51 |
|         PublishAsync |   10240 (10 kB) |    167.26 |  0.99 |          - |          - |          - |         580.95 |
|                      |                 |           |       |            |            |            |                |
|   **PublishBaseLibrary** | **102400 (100 kB)** |    **263.83** |  **1.00** | **30000.0000** | **30000.0000** | **30000.0000** |      **100775.53** |
| PublishParallelAsync | 102400 (100 kB) |    237.81 |  0.90 |          - |          - |          - |         480.07 |
|         PublishAsync | 102400 (100 kB) |    200.86 |  0.76 |          - |          - |          - |         631.87 |
|                      |                 |           |       |            |            |            |                |
|   **PublishBaseLibrary** | **204800 (200 kB)** |    **421.49** |  **1.00** | **55000.0000** | **55000.0000** | **55000.0000** |      **201463.16** |
| PublishParallelAsync | 204800 (200 kB) |    229.39 |  0.54 |          - |          - |          - |         608.84 |
|         PublishAsync | 204800 (200 kB) |    293.76 |  0.70 |          - |          - |          - |         770.74 |

## Legend

Each test performing publishing of 1000 messages, with payload size as per Parameters.

**PublishBaseLibrary** - publish via the official RabbitMQ-client, baseline benchmark.

**PublishParallelAsync** - publish via RabbitMQ.Next library via single publisher called from 10 threads in parallel.

**PublishAsync** - publish via RabbitMQ.Next library via single publisher called from single thread.