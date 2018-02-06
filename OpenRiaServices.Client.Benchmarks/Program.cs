using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Jobs;
using Cities;
using System.Threading;
using ClientBenchmarks.Helpers;

namespace ClientBenchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.Run<EntityBenchmarks>();
            //BenchmarkRunner.Run<EntitySetBenchmarks>();
            BenchmarkRunner.Run<ChangeSetBenchmarks>();
        }
    }
}
