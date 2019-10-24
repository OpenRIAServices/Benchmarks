``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
Intel Core i5-2500K CPU 3.30GHz (Sandy Bridge), 1 CPU, 4 logical and 4 physical cores
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
  [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.8.4010.0
  DefaultJob : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.8.4010.0


```
|                     Method | NumEntities | DomainClient | total | concurrent | depth |       Mean |     Error |    StdDev |    Gen 0 |    Gen 1 |  Gen 2 | Allocated |
|--------------------------- |------------ |------------- |------ |----------- |------ |-----------:|----------:|----------:|---------:|---------:|-------:|----------:|
| **RunBenchmarksAsyncParallel** |          **10** |    **WcfBinary** |    **40** |          **1** |     **?** |   **454.0 us** |  **8.911 us** |  **8.752 us** |  **17.1875** |        **-** |      **-** |  **54.78 KB** |
|         **PipelinedLoadAsync** |          **10** |    **WcfBinary** |    **40** |          **?** |     **1** |   **450.4 us** |  **6.753 us** |  **6.317 us** |  **17.1875** |        **-** |      **-** |  **54.56 KB** |
| **RunBenchmarksAsyncParallel** |          **10** |    **WcfBinary** |    **40** |          **2** |     **?** |   **299.1 us** |  **1.433 us** |  **1.270 us** |  **17.5781** |   **0.7813** |      **-** |   **55.5 KB** |
|         **PipelinedLoadAsync** |          **10** |    **WcfBinary** |    **40** |          **?** |     **2** |   **243.0 us** |  **4.802 us** |  **7.890 us** |  **17.5781** |   **2.3438** |      **-** |  **55.26 KB** |
| **RunBenchmarksAsyncParallel** |          **10** |    **WcfBinary** |    **40** |          **4** |     **?** |   **244.9 us** |  **2.419 us** |  **2.263 us** |  **17.1875** |   **2.7344** |      **-** |  **55.43 KB** |
|         **PipelinedLoadAsync** |          **10** |    **WcfBinary** |    **40** |          **?** |     **4** |   **186.3 us** |  **2.978 us** |  **2.486 us** |  **16.7969** |   **3.9063** |      **-** |  **55.41 KB** |
|    **GetCititesUniqueContext** |          **10** |    **WcfBinary** |     **?** |          **?** |     **?** |   **507.3 us** |  **9.926 us** | **14.550 us** |  **20.0195** |   **0.4883** |      **-** |  **63.44 KB** |
|     GetCititesReuseContext |          10 |    WcfBinary |     ? |          ? |     ? |   451.1 us |  8.870 us | 13.277 us |  17.5781 |        - |      - |  54.84 KB |
|                InvokeAsync |          10 |    WcfBinary |     ? |          ? |     ? |   459.0 us |  7.499 us |  7.015 us |  15.1367 |        - |      - |  47.63 KB |
| **RunBenchmarksAsyncParallel** |         **100** |    **WcfBinary** |    **40** |          **1** |     **?** |   **912.9 us** | **18.090 us** | **22.878 us** |  **33.9286** |   **7.1429** |      **-** | **117.13 KB** |
|         **PipelinedLoadAsync** |         **100** |    **WcfBinary** |    **40** |          **?** |     **1** |   **946.8 us** | **18.651 us** | **22.905 us** |  **33.9286** |   **7.1429** |      **-** | **117.01 KB** |
| **RunBenchmarksAsyncParallel** |         **100** |    **WcfBinary** |    **40** |          **2** |     **?** |   **508.5 us** |  **5.198 us** |  **4.862 us** |  **32.0313** |  **10.1563** |      **-** | **127.57 KB** |
|         **PipelinedLoadAsync** |         **100** |    **WcfBinary** |    **40** |          **?** |     **2** |   **448.7 us** |  **2.252 us** |  **1.881 us** |  **32.0313** |   **9.3750** |      **-** | **127.79 KB** |
| **RunBenchmarksAsyncParallel** |         **100** |    **WcfBinary** |    **40** |          **4** |     **?** |   **387.6 us** |  **6.167 us** |  **5.768 us** |  **30.4688** |  **10.1563** |      **-** | **117.68 KB** |
|         **PipelinedLoadAsync** |         **100** |    **WcfBinary** |    **40** |          **?** |     **4** |   **339.5 us** |  **6.755 us** | **11.098 us** |  **32.8125** |  **13.2813** |      **-** | **118.45 KB** |
|    **GetCititesUniqueContext** |         **100** |    **WcfBinary** |     **?** |          **?** |     **?** | **1,030.2 us** | **19.025 us** | **18.685 us** |  **39.0625** |  **11.7188** |      **-** | **153.87 KB** |
|     GetCititesReuseContext |         100 |    WcfBinary |     ? |          ? |     ? |   950.4 us | 18.998 us | 32.260 us |  35.1563 |   7.8125 |      - | 118.56 KB |
|                InvokeAsync |         100 |    WcfBinary |     ? |          ? |     ? |   467.2 us |  1.991 us |  1.863 us |  15.1367 |        - |      - |  47.69 KB |
| **RunBenchmarksAsyncParallel** |        **1000** |    **WcfBinary** |    **40** |          **1** |     **?** | **4,564.8 us** | **20.837 us** | **19.491 us** | **208.3333** |  **83.3333** |      **-** | **700.92 KB** |
|         **PipelinedLoadAsync** |        **1000** |    **WcfBinary** |    **40** |          **?** |     **1** | **4,571.2 us** | **25.822 us** | **24.154 us** | **200.0000** |  **75.0000** |      **-** | **699.95 KB** |
| **RunBenchmarksAsyncParallel** |        **1000** |    **WcfBinary** |    **40** |          **2** |     **?** | **2,706.7 us** | **19.268 us** | **18.023 us** | **195.0000** |  **75.0000** |      **-** | **712.61 KB** |
|         **PipelinedLoadAsync** |        **1000** |    **WcfBinary** |    **40** |          **?** |     **2** | **2,501.1 us** | **15.689 us** | **14.675 us** | **205.0000** |  **85.0000** |      **-** | **715.99 KB** |
| **RunBenchmarksAsyncParallel** |        **1000** |    **WcfBinary** |    **40** |          **4** |     **?** | **2,175.9 us** | **28.530 us** | **25.292 us** | **220.8333** | **100.0000** | **8.3333** | **753.17 KB** |
|         **PipelinedLoadAsync** |        **1000** |    **WcfBinary** |    **40** |          **?** |     **4** | **2,004.6 us** | **37.939 us** | **40.594 us** | **228.5714** | **110.7143** | **7.1429** | **757.14 KB** |
|    **GetCititesUniqueContext** |        **1000** |    **WcfBinary** |     **?** |          **?** |     **?** | **5,302.1 us** | **17.592 us** | **16.456 us** | **312.5000** | **156.2500** |      **-** | **967.04 KB** |
|     GetCititesReuseContext |        1000 |    WcfBinary |     ? |          ? |     ? | 4,559.4 us | 19.809 us | 18.530 us | 210.9375 |  78.1250 |      - | 700.38 KB |
|                InvokeAsync |        1000 |    WcfBinary |     ? |          ? |     ? |   473.4 us |  5.404 us |  4.790 us |  15.1367 |        - |      - |  47.72 KB |
