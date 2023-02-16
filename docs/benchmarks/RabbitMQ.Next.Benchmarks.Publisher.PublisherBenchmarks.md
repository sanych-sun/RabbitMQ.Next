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
|   PublishBaseLibrary |     1024 (1 kB) |    155.87 |  1.00 |          - |          - |          - |         1496.7 |
| PublishParallelAsync |     1024 (1 kB) |     57.25 |  0.37 |          - |          - |          - |         437.28 |
|         PublishAsync |     1024 (1 kB) |    165.44 |  1.06 |          - |          - |          - |         580.95 |
|                      |                 |           |       |            |            |            |                |
|   PublishBaseLibrary |   10240 (10 kB) |    177.25 |  1.00 |  2000.0000 |          - |          - |        10496.7 |
| PublishParallelAsync |   10240 (10 kB) |     63.99 |  0.37 |          - |          - |          - |         437.48 |
|         PublishAsync |   10240 (10 kB) |    166.57 |  0.96 |          - |          - |          - |          581.2 |
|                      |                 |           |       |            |            |            |                |
|   PublishBaseLibrary | 102400 (100 kB) |    266.97 |  1.00 | 29000.0000 | 29000.0000 | 29000.0000 |      100772.52 |
| PublishParallelAsync | 102400 (100 kB) |    210.33 |  0.79 |          - |          - |          - |         475.34 |
|         PublishAsync | 102400 (100 kB) |    209.71 |  0.79 |          - |          - |          - |         635.95 |
|                      |                 |           |       |            |            |            |                |
|   PublishBaseLibrary | 204800 (200 kB) |    430.12 |  1.00 | 55000.0000 | 55000.0000 | 55000.0000 |      201384.56 |
| PublishParallelAsync | 204800 (200 kB) |    258.07 |  0.60 |          - |          - |          - |          614.1 |
|         PublishAsync | 204800 (200 kB) |    307.29 |  0.71 |          - |          - |          - |         770.99 |

## Legend

Each test performing publishing of 1000 messages, with payload size as per Parameters.

**PublishBaseLibrary** - publish via the official RabbitMQ-client, baseline benchmark.

**PublishParallelAsync** - publish via RabbitMQ.Next library via single publisher called from 10 threads in parallel.

**PublishAsync** - publish via RabbitMQ.Next library via single publisher called from single thread.