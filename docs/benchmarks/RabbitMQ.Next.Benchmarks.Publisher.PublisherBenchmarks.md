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
|   **PublishBaseLibrary** |     **1024 (1 kB)** | **160.01 ms** | **12.850 ms** |  **8.500 ms** |  **1.00** |    **0.00** |          **-** |          **-** |          **-** |   **1,497 KB** |
| PublishParallelAsync |     1024 (1 kB) |  60.66 ms |  5.980 ms |  3.956 ms |  0.38 |    0.04 |          - |          - |          - |     440 KB |
|         PublishAsync |     1024 (1 kB) | 161.64 ms |  5.629 ms |  3.723 ms |  1.01 |    0.04 |          - |          - |          - |     583 KB |
|                      |                 |           |           |           |       |         |            |            |            |            |
|   **PublishBaseLibrary** |   **10240 (10 kB)** | **166.70 ms** |  **5.865 ms** |  **3.879 ms** |  **1.00** |    **0.00** |  **2000.0000** |          **-** |          **-** |  **10,497 KB** |
| PublishParallelAsync |   10240 (10 kB) |  65.79 ms |  7.756 ms |  5.130 ms |  0.39 |    0.03 |          - |          - |          - |     442 KB |
|         PublishAsync |   10240 (10 kB) | 178.59 ms | 24.755 ms | 16.374 ms |  1.07 |    0.09 |          - |          - |          - |     583 KB |
|                      |                 |           |           |           |       |         |            |            |            |            |
|   **PublishBaseLibrary** | **102400 (100 kB)** | **259.67 ms** | **16.617 ms** | **10.991 ms** |  **1.00** |    **0.00** | **29000.0000** | **29000.0000** | **29000.0000** | **100,778 KB** |
| PublishParallelAsync | 102400 (100 kB) | 225.54 ms | 70.446 ms | 46.596 ms |  0.87 |    0.19 |          - |          - |          - |     515 KB |
|         PublishAsync | 102400 (100 kB) | 217.56 ms | 20.267 ms | 13.405 ms |  0.84 |    0.06 |          - |          - |          - |     635 KB |
|                      |                 |           |           |           |       |         |            |            |            |            |
|   **PublishBaseLibrary** | **204800 (200 kB)** | **414.82 ms** |  **8.919 ms** |  **5.899 ms** |  **1.00** |    **0.00** | **54000.0000** | **54000.0000** | **54000.0000** | **201,184 KB** |
| PublishParallelAsync | 204800 (200 kB) | 268.94 ms | 91.589 ms | 60.580 ms |  0.65 |    0.15 |          - |          - |          - |     619 KB |
|         PublishAsync | 204800 (200 kB) | 332.16 ms | 25.772 ms | 17.046 ms |  0.80 |    0.04 |          - |          - |          - |     775 KB |

## Legend

Each test performing publishing of 1000 messages, with payload size as per Parameters.

PublishBaseLibrary - publish via the official RabbitMQ-client, baseline benchmark.
PublishParallelAsync - publish via RabbitMQ.Next library via single publisher called from 10 threads in parallel.
PublishAsync - publish via RabbitMQ.Next library via single publisher called from single thread.