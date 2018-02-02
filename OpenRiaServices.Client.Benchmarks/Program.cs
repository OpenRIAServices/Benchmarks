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

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var i = new InvokeBenchmarks();
            i.MetaMember_VirtualMethod();
            i.DelegateInvoke();


            //BenchmarkRunner.Run<EntityBenchmarks>();
            //BenchmarkRunner.Run<EntitySetBenchmarks>();
            BenchmarkRunner.Run<InvokeBenchmarks>();
        }
    }
}
