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
|               Method |      parameters |      Mean |     Error |    StdDev | Ratio | RatioSD |      Gen 0 |      Gen 1 |      Gen 2 |  Allocated |
|--------------------- |---------------- |----------:|----------:|----------:|------:|--------:|-----------:|-----------:|-----------:|-----------:|
|   **PublishBaseLibrary** |     **1024 (1 kB)** | **158.73 ms** |  **8.079 ms** |  **5.344 ms** |  **1.00** |    **0.00** |          **-** |          **-** |          **-** |   **1,497 KB** |
| PublishParallelAsync |     1024 (1 kB) |  61.40 ms |  8.356 ms |  5.527 ms |  0.39 |    0.03 |          - |          - |          - |     437 KB |
|         PublishAsync |     1024 (1 kB) | 164.94 ms | 10.502 ms |  6.946 ms |  1.04 |    0.05 |          - |          - |          - |     581 KB |
|                      |                 |           |           |           |       |         |            |            |            |            |
|   **PublishBaseLibrary** |   **10240 (10 kB)** | **168.93 ms** |  **7.490 ms** |  **4.954 ms** |  **1.00** |    **0.00** |  **2000.0000** |          **-** |          **-** |  **10,497 KB** |
| PublishParallelAsync |   10240 (10 kB) |  66.63 ms | 10.812 ms |  7.151 ms |  0.40 |    0.05 |          - |          - |          - |     437 KB |
|         PublishAsync |   10240 (10 kB) | 166.83 ms |  6.206 ms |  4.105 ms |  0.99 |    0.05 |          - |          - |          - |     581 KB |
|                      |                 |           |           |           |       |         |            |            |            |            |
|   **PublishBaseLibrary** | **102400 (100 kB)** | **264.39 ms** | **22.057 ms** | **14.589 ms** |  **1.00** |    **0.00** | **29000.0000** | **29000.0000** | **29000.0000** | **100,768 KB** |
| PublishParallelAsync | 102400 (100 kB) | 215.02 ms | 30.281 ms | 20.029 ms |  0.82 |    0.10 |          - |          - |          - |     472 KB |
|         PublishAsync | 102400 (100 kB) | 207.20 ms | 11.773 ms |  7.787 ms |  0.79 |    0.05 |          - |          - |          - |     631 KB |
|                      |                 |           |           |           |       |         |            |            |            |            |
|   **PublishBaseLibrary** | **204800 (200 kB)** | **426.52 ms** | **19.988 ms** | **13.221 ms** |  **1.00** |    **0.00** | **56000.0000** | **56000.0000** | **56000.0000** | **201,276 KB** |
| PublishParallelAsync | 204800 (200 kB) | 233.32 ms | 62.876 ms | 41.589 ms |  0.55 |    0.10 |          - |          - |          - |     634 KB |
|         PublishAsync | 204800 (200 kB) | 299.04 ms | 15.344 ms | 10.149 ms |  0.70 |    0.03 |          - |          - |          - |     771 KB |

## Legend

Each test performing publishing of 1000 messages, with payload size as per Parameters.

PublishBaseLibrary - publish via the official RabbitMQ-client, baseline benchmark.
PublishParallelAsync - publish via RabbitMQ.Next library via single publisher called from 10 threads in parallel.
PublishAsync - publish via RabbitMQ.Next library via single publisher called from single thread.