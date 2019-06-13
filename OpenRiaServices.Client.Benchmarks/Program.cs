using BenchmarkDotNet.Running;
using ClientBenchmarks.Server;
using OpenRiaServices.Client.Benchmarks.Client.Cities;
using OpenRiaServices.DomainServices.Client;
using System.Threading.Tasks;

namespace ClientBenchmarks
{
    internal class Program
    {
        static bool onlyProfiling = false;

        private static void Main(string[] args)
        {
            if (onlyProfiling)
            {
                //BenchmarkRunner.Run<E2Ebenchmarks>();
                //return;
                Task.Run(() => RunBenchmarksAsyncParallel()).Wait();
                return;

                const int num = 1;
                for (int i = 0; i < 1; ++i)
                {
                    var a = new LoadBenchmarks();
                    a.NumEntities = num;
                    a.LoadEntities();
                    a.LoadAndRefreshEntities();
                    a.LoadAndMergeEntities();

                    var s = new ChangeSetBenchmarks();
                    s.NumEntities = num;
                    s.SubmitAdded();

                    var es = new EntitySetBenchmarks();
                    es.NumEntities = num;
                    es.Add();
                    es.AddAndDetach();
                    es.AddAndRemove();
                    es.Attach();
                    es.AttachAndDetach();
                    es.AttachAndModify();
                    es.AttachAndRemove();
                }
            }
            else
            {
                BenchmarkRunner.Run<E2Ebenchmarks>();
                //BenchmarkRunner.Run<EntityBenchmarks>();
                //BenchmarkRunner.Run<EntitySetBenchmarks>();
                //BenchmarkRunner.Run<ChangeSetBenchmarks>();
                //BenchmarkRunner.Run<LoadBenchmarks>();
            }
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

        private static async Task RunBenchmarksAsyncParallel()
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
