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
using OpenRiaServices.DomainServices.Client;

namespace ClientBenchmarks
{
    class Program
    {
        public static void Main(string[] args)
        {
            //BenchmarkRunner.Run<E2Ebenchmarks>();
            //return;
            Task.Run(() => RunBenchmarksAsyncParallel()).Wait();
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

        private static async Task RunBenchmarksAsync()
        {
            var b = new E2Ebenchmarks();
            b.DomainClient = DomainClientType.WcfBinary;
            b.Start();

            for (int i = 0; i < 1000; ++i)
            {
                // We don't have a sync context so continuations can run in background
                await b.GetCititesReuseContext().ConfigureAwait(false);
            }

            b.Stop();
        }

        private static async Task  RunBenchmarksAsyncParallel()
        {
            var b = new E2Ebenchmarks();
            b.DomainClient = DomainClientType.HttpBinary;
            b.Start();

            const int total = 1000;
            const int concurrent = 8;
            const int outer = total / concurrent;

            var tasks = new Task<LoadResult<City>>[concurrent];

            for (int i = 0; i < outer; ++i)
            {
                for (int j = 0; j < concurrent; ++j)
                    tasks[j] = b.GetCititesReuseContext();

                // We don't have a sync context so continuations can run in background
                var results = await Task.WhenAll(tasks);
            }

            b.Stop();
        }
    }
}
