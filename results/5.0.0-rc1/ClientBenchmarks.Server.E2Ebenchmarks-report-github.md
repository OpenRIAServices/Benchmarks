``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.19041
Intel Core i5-2500K CPU 3.30GHz (Sandy Bridge), 1 CPU, 4 logical and 4 physical cores
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
  [Host]     : .NET Framework 4.8 (4.8.4250.0), X64 RyuJIT
  DefaultJob : .NET Framework 4.8 (4.8.4250.0), X64 RyuJIT


```
|                  Method | NumEntities | DomainClient |       Mean |    Error |   StdDev |    Gen 0 |    Gen 1 | Gen 2 | Allocated |
|------------------------ |------------ |------------- |-----------:|---------:|---------:|---------:|---------:|------:|----------:|
| **GetCititesUniqueContext** |          **10** |    **WcfBinary** |   **539.1 us** |  **9.77 us** |  **8.66 us** |  **19.5313** |        **-** |     **-** |  **62.95 KB** |
|  GetCititesReuseContext |          10 |    WcfBinary |   503.1 us |  5.72 us |  5.35 us |  17.5781 |        - |     - |  54.23 KB |
|             InvokeAsync |          10 |    WcfBinary |   500.4 us |  7.79 us |  7.29 us |  14.6484 |        - |     - |  47.23 KB |
| **GetCititesUniqueContext** |         **100** |    **WcfBinary** | **1,047.0 us** | **18.15 us** | **16.98 us** |  **37.1094** |  **11.7188** |     **-** | **146.49 KB** |
|  GetCititesReuseContext |         100 |    WcfBinary |   992.3 us | 19.22 us | 23.60 us |  34.1797 |   5.8594 |     - |  115.1 KB |
|             InvokeAsync |         100 |    WcfBinary |   518.1 us |  7.47 us |  6.98 us |  14.6484 |        - |     - |  47.34 KB |
| **GetCititesUniqueContext** |        **1000** |    **WcfBinary** | **5,437.6 us** | **40.25 us** | **35.68 us** | **312.5000** | **148.4375** |     **-** | **961.66 KB** |
|  GetCititesReuseContext |        1000 |    WcfBinary | 4,572.1 us | 26.16 us | 24.47 us | 218.7500 |  62.5000 |     - | 697.95 KB |
|             InvokeAsync |        1000 |    WcfBinary |   513.3 us |  8.74 us |  8.17 us |  13.6719 |   0.9766 |     - |   47.3 KB |
