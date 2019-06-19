``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.17763.557 (1809/October2018Update/Redstone5)
Intel Core i5-2500K CPU 3.30GHz (Sandy Bridge), 1 CPU, 4 logical and 4 physical cores
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
  [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3416.0
  Job-FEFVFA : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3416.0

InvocationCount=1  UnrollFactor=1  

```
|                     Method | NumEntities | DomainClient | total | concurrent | depth |        Mean |      Error |     StdDev |      Median |     Gen 0 |   Gen 1 | Gen 2 |  Allocated |
|--------------------------- |------------ |------------- |------ |----------- |------ |------------:|-----------:|-----------:|------------:|----------:|--------:|------:|-----------:|
| **RunBenchmarksAsyncParallel** |         **100** |    **WcfBinary** |   **400** |          **1** |     **?** |  **1,027.1 us** |  **10.909 us** |  **10.205 us** |  **1,022.1 us** |   **65.0000** |  **2.5000** |     **-** |  **215.85 KB** |
|         **PipelinedLoadAsync** |         **100** |    **WcfBinary** |   **400** |          **?** |     **1** |  **1,014.7 us** |  **20.286 us** |  **27.768 us** |  **1,020.7 us** |   **65.0000** |  **2.5000** |     **-** |  **214.29 KB** |
| **RunBenchmarksAsyncParallel** |         **100** |    **WcfBinary** |   **400** |          **2** |     **?** |    **550.5 us** |   **7.319 us** |   **6.488 us** |    **551.6 us** |   **60.0000** |  **2.5000** |     **-** |  **219.24 KB** |
|         **PipelinedLoadAsync** |         **100** |    **WcfBinary** |   **400** |          **?** |     **2** |    **492.8 us** |   **7.826 us** |   **6.937 us** |    **491.6 us** |   **57.5000** |  **2.5000** |     **-** |  **219.42 KB** |
| **RunBenchmarksAsyncParallel** |         **100** |    **WcfBinary** |   **400** |          **4** |     **?** |    **437.8 us** |   **5.996 us** |   **5.315 us** |    **436.5 us** |   **60.0000** |  **7.5000** |     **-** |  **222.89 KB** |
|         **PipelinedLoadAsync** |         **100** |    **WcfBinary** |   **400** |          **?** |     **4** |    **388.5 us** |   **7.703 us** |   **7.910 us** |    **389.6 us** |   **57.5000** | **10.0000** |     **-** |  **223.57 KB** |
|    **GetCititesUniqueContext** |         **100** |    **WcfBinary** |     **?** |          **?** |     **?** |  **3,978.3 us** |  **79.145 us** | **110.950 us** |  **3,934.2 us** |         **-** |       **-** |     **-** |   **895.7 KB** |
|     GetCititesReuseContext |         100 |    WcfBinary |     ? |          ? |     ? |  1,907.1 us |  38.023 us |  73.258 us |  1,899.4 us |         - |       - |     - |   343.7 KB |
|                     Submit |         100 |    WcfBinary |     ? |          ? |     ? | 13,704.6 us | 334.139 us | 936.962 us | 14,038.9 us | 1000.0000 |       - |     - | 3739.05 KB |
