``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.17763.557 (1809/October2018Update/Redstone5)
Intel Core i5-2500K CPU 3.30GHz (Sandy Bridge), 1 CPU, 4 logical and 4 physical cores
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
  [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3416.0
  Job-FEFVFA : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3416.0

InvocationCount=1  UnrollFactor=1  

```
|                     Method | NumEntities | DomainClient | total | concurrent | depth |        Mean |      Error |       StdDev |      Median |     Gen 0 |   Gen 1 | Gen 2 |  Allocated |
|--------------------------- |------------ |------------- |------ |----------- |------ |------------:|-----------:|-------------:|------------:|----------:|--------:|------:|-----------:|
| **RunBenchmarksAsyncParallel** |         **100** |    **WcfBinary** |   **400** |          **1** |     **?** |  **1,005.3 us** |  **19.642 us** |    **20.170 us** |  **1,012.8 us** |   **62.5000** |  **2.5000** |     **-** |  **214.83 KB** |
|         **PipelinedLoadAsync** |         **100** |    **WcfBinary** |   **400** |          **?** |     **1** |  **1,007.3 us** |  **12.534 us** |    **11.111 us** |  **1,004.8 us** |   **65.0000** |  **2.5000** |     **-** |  **214.72 KB** |
| **RunBenchmarksAsyncParallel** |         **100** |    **WcfBinary** |   **400** |          **2** |     **?** |    **563.9 us** |  **10.742 us** |    **11.494 us** |    **564.3 us** |   **57.5000** |  **2.5000** |     **-** |  **218.96 KB** |
|         **PipelinedLoadAsync** |         **100** |    **WcfBinary** |   **400** |          **?** |     **2** |    **512.8 us** |  **10.974 us** |    **26.504 us** |    **504.9 us** |   **57.5000** |  **2.5000** |     **-** |  **219.38 KB** |
| **RunBenchmarksAsyncParallel** |         **100** |    **WcfBinary** |   **400** |          **4** |     **?** |    **432.6 us** |   **5.999 us** |     **5.318 us** |    **432.6 us** |   **55.0000** |  **7.5000** |     **-** |  **221.87 KB** |
|         **PipelinedLoadAsync** |         **100** |    **WcfBinary** |   **400** |          **?** |     **4** |    **383.9 us** |   **7.166 us** |     **7.038 us** |    **384.4 us** |   **57.5000** | **10.0000** |     **-** |  **223.48 KB** |
|    **GetCititesUniqueContext** |         **100** |    **WcfBinary** |     **?** |          **?** |     **?** |  **3,773.9 us** |  **73.435 us** |    **78.574 us** |  **3,795.8 us** |         **-** |       **-** |     **-** |   **887.7 KB** |
|     GetCititesReuseContext |         100 |    WcfBinary |     ? |          ? |     ? |  1,905.9 us |  37.855 us |    88.484 us |  1,890.0 us |         - |       - |     - |   319.7 KB |
|                     Submit |         100 |    WcfBinary |     ? |          ? |     ? | 13,610.3 us | 423.841 us | 1,202.367 us | 13,947.4 us | 1000.0000 |       - |     - | 3741.66 KB |
