extern alias server;

using BenchmarkDotNet.Running;
using ClientBenchmarks.Server;
using OpenRiaServices.Client.Benchmarks.Client.Cities;
using OpenRiaServices.DomainServices.Client;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using server::OpenRiaServices.DomainServices.Hosting;
using OpenRiaServices.Client.Benchmarks.Server.Cities;
using System.ServiceModel.Activation;

namespace ClientBenchmarks
{
    internal class Program
    {
        static bool onlyProfiling = false;

        private static void Main(string[] args)
        {
            if (args.Length > 0 && args.Contains("server"))
            {
                Uri uri = new Uri($"http://localhost/Temporary_Listen_Addresses/Cities/Services/Citites.svc");
                CityDomainService.GetCitiesResult = E2Ebenchmarks.CreateValidCities(100).ToList();
                StartServerAndWaitForKey(uri, typeof(CityDomainService));
                return;
            }

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
                BenchmarkRunner.Run<LoadBenchmarks>();
                //BenchmarkRunner.Run<E2Ebenchmarks>();
                //BenchmarkRunner.Run<EntityBenchmarks>();
                //BenchmarkRunner.Run<EntitySetBenchmarks>();
                //BenchmarkRunner.Run<ChangeSetBenchmarks>();

            }
        }

        private static void StartServerAndWaitForKey(Uri uri, Type type)
        {
            using (var host = new DomainServiceHost(type, uri))
            {
                //other relevent code to configure host's end point etc
                if (host.Description.Behaviors.Contains(typeof(AspNetCompatibilityRequirementsAttribute)))
                {
                    var compatibilityRequirementsAttribute = host.Description.Behaviors[typeof(AspNetCompatibilityRequirementsAttribute)] as AspNetCompatibilityRequirementsAttribute;
                    compatibilityRequirementsAttribute.RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed;
                }

                host.Open();
                Console.WriteLine("Press ENTER to exit");
                Console.ReadLine();
                host.Close();
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
