using BenchmarkDotNet.Running;
using ClientBenchmarks.Server;
using OpenRiaServices.Client.Benchmarks.Client.Cities;
using OpenRiaServices.DomainServices.Client;
using System;
using System.Diagnostics;
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
                Task.Run(() => RunBenchmarksAsyncParallel()).Wait();

                Task.Run(() => RunPipelined()).Wait();
                Console.WriteLine("PAUSE");
                Console.ReadLine();
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

        private static async Task RunBenchmarksAsyncParallel()
        {
            var b = new E2Ebenchmarks();
            b.DomainClient = DomainClientType.WcfBinary;
            b.Start();

            await b.GetCititesReuseContext();

            var sw = Stopwatch.StartNew();
            await b.RunBenchmarksAsyncParallel(100, 2);

            Console.WriteLine("RunBenchmarksAsyncParallel elapsed time is {0}", sw.Elapsed);
            b.Stop();
        }
        private static async Task RunPipelined()
        {
            var b = new E2Ebenchmarks();
            b.DomainClient = DomainClientType.WcfBinary;
            b.Start();

            await b.GetCititesReuseContext();

            var sw = Stopwatch.StartNew();

            await b.PipelinedLoadAsync(100, 2);

            Console.WriteLine("RunPipelined elapsed time is {0} for 2 ", sw.Elapsed);


            sw = Stopwatch.StartNew();

            await b.PipelinedLoadAsync(1000, 4);

            Console.WriteLine("RunPipelined elapsed time is {0} for 4 ", sw.Elapsed);

            b.Stop();
        }
    }
}
