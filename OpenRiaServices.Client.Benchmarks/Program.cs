using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Jobs;
using OpenRiaServices.Client.Benchmarks.Client.Cities;
using System.Threading;
using ClientBenchmarks.Helpers;
using ClientBenchmarks.Server;

namespace ClientBenchmarks
{
    class Program
    {
        public static void Main(string[] args)
        {
            //BenchmarkRunner.Run<E2Ebenchmarks>();
            //return;
            Task.Run(() => RunBenchmarks()).GetAwaiter().GetResult();
            return;

            var a = new LoadBenchmarks();
            a.NumEntities = 5000;
            a.LoadEntities();
            a.LoadAndRefreshEntities();
            a.LoadAndMergeEntities();
            return;

            //BenchmarkRunner.Run<EntityBenchmarks>();
            //BenchmarkRunner.Run<EntitySetBenchmarks>();
            BenchmarkRunner.Run<ChangeSetBenchmarks>();
            BenchmarkRunner.Run<LoadBenchmarks>();
        }

        private static void RunBenchmarks()
        {
            var b = new E2Ebenchmarks();
            b.DomainClient = DomainClientType.HttpBinary;
            b.Start();

            for (int i = 0; i < 1000; ++i)
            {
                // We don't have a sync context so continuations can run in background
                 b.GetCititesReuseContext().GetAwaiter().GetResult();
            }

            b.Stop();
        }
    }
}
