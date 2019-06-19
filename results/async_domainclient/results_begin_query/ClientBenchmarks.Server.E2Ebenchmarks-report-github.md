``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.17763.557 (1809/October2018Update/Redstone5)
Intel Core i5-2500K CPU 3.30GHz (Sandy Bridge), 1 CPU, 4 logical and 4 physical cores
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
  [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3416.0
  Job-FEFVFA : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3416.0

InvocationCount=1  UnrollFactor=1  

```
|                     Method | NumEntities | DomainClient | total | concurrent | depth |        Mean |      Error |       StdDev |      Median |     Gen 0 |  Gen 1 | Gen 2 | Allocated |
|--------------------------- |------------ |------------- |------ |----------- |------ |------------:|-----------:|-------------:|------------:|----------:|-------:|------:|----------:|
| **RunBenchmarksAsyncParallel** |         **100** |    **WcfBinary** |   **400** |          **1** |     **?** |  **1,033.1 us** |   **6.008 us** |     **5.619 us** |  **1,032.5 us** |   **62.5000** | **2.5000** |     **-** | **212.05 KB** |
|         **PipelinedLoadAsync** |         **100** |    **WcfBinary** |   **400** |          **?** |     **1** |  **1,019.2 us** |   **4.486 us** |     **3.746 us** |  **1,018.2 us** |   **62.5000** | **2.5000** |     **-** | **212.07 KB** |
| **RunBenchmarksAsyncParallel** |         **100** |    **WcfBinary** |   **400** |          **2** |     **?** |    **723.5 us** |   **3.509 us** |     **3.111 us** |    **723.2 us** |   **57.5000** | **2.5000** |     **-** | **215.82 KB** |
|         **PipelinedLoadAsync** |         **100** |    **WcfBinary** |   **400** |          **?** |     **2** |    **520.0 us** |   **9.988 us** |    **10.257 us** |    **517.8 us** |   **55.0000** | **2.5000** |     **-** | **217.65 KB** |
| **RunBenchmarksAsyncParallel** |         **100** |    **WcfBinary** |   **400** |          **4** |     **?** |    **554.9 us** |  **10.536 us** |     **8.798 us** |    **553.6 us** |   **52.5000** | **2.5000** |     **-** | **219.75 KB** |
|         **PipelinedLoadAsync** |         **100** |    **WcfBinary** |   **400** |          **?** |     **4** |    **438.0 us** |   **3.816 us** |     **3.383 us** |    **437.7 us** |   **55.0000** | **7.5000** |     **-** | **220.17 KB** |
|    **GetCititesUniqueContext** |         **100** |    **WcfBinary** |     **?** |          **?** |     **?** |  **3,946.3 us** |  **74.449 us** |    **62.168 us** |  **3,933.9 us** |         **-** |      **-** |     **-** |  **887.7 KB** |
|     GetCititesReuseContext |         100 |    WcfBinary |     ? |          ? |     ? |  2,033.7 us |  62.843 us |   164.449 us |  1,992.2 us |         - |      - |     - |  327.7 KB |
|                     Submit |         100 |    WcfBinary |     ? |          ? |     ? | 13,806.5 us | 388.498 us | 1,095.765 us | 14,113.6 us | 1000.0000 |      - |     - | 3731.3 KB |
