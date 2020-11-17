extern alias server;

using BenchmarkDotNet.Running;
using ClientBenchmarks.Server;
using OpenRiaServices.Client.Benchmarks.Client.Cities;
using OpenRiaServices.Client;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using OpenRiaServices.Client.Benchmarks.Server.Cities;
using System.ServiceModel.Activation;
using ClientBenchmarks.Server.Example;
using System.Configuration;
using server::OpenRiaServices.Hosting.Wcf;
using server::OpenRiaServices.Hosting.Wcf.Configuration;

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
                CityDomainService.GetCitiesResult = E2Ebenchmarks.CreateValidCities(1).ToList();
                StartServerAndWaitForKey(uri, typeof(ExampelService));
                StartServerAndWaitForKey(uri, typeof(CityDomainService));
                return;
            }

            if (onlyProfiling || (args.Length > 0 && args[0].Contains("profile")))
            {
                //Task.Run(() => RunBenchmarksAsyncParallel()).Wait();

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
                BenchmarkRunner.Run<E2Ebenchmarks>();
                //BenchmarkRunner.Run<EntityBenchmarks>();
                //BenchmarkRunner.Run<EntitySetBenchmarks>();
                //BenchmarkRunner.Run<ChangeSetBenchmarks>();
            }
        }

        private static void StartServerAndWaitForKey(Uri uri, Type type)
        {
            if (DomainServicesSection.Current.Endpoints.Count == 1)
            {
                DomainServicesSection.Current.Endpoints.Add(
                    new ProviderSettings("soap", typeof(SoapXmlEndpointFactory).AssemblyQualifiedName));
                DomainServicesSection.Current.Endpoints.Add(
                    new ProviderSettings("json", typeof(JsonEndpointFactory).AssemblyQualifiedName));
            }

            using (var host = new DomainServiceHost(type, uri))
            {
                //other relevent code to configure host's end point etc
                if (host.Description.Behaviors.Contains(typeof(AspNetCompatibilityRequirementsAttribute)))
                {
                    var compatibilityRequirementsAttribute = host.Description.Behaviors[typeof(AspNetCompatibilityRequirementsAttribute)] as AspNetCompatibilityRequirementsAttribute;
                    compatibilityRequirementsAttribute.RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed;
                }

                host.Open();

                Console.WriteLine($"DomainService {type.Name} running at {uri}");
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
            b.NumEntities = 10;
            b.DomainClient = DomainClientType.WcfBinary;
            b.Start();

            await b.GetCititesReuseContext();

            Console.WriteLine("PRESS KEY TO START");
            Console.ReadLine();

            var sw = Stopwatch.StartNew();

            await b.PipelinedLoadAsync(5000, 20);

            Console.WriteLine("RunPipelined elapsed time is {0} for 2 ", sw.Elapsed);


            sw = Stopwatch.StartNew();

            await b.PipelinedLoadAsync(5000, 50);

            Console.WriteLine("RunPipelined elapsed time is {0} for 4 ", sw.Elapsed);

            sw = Stopwatch.StartNew();

            await b.PipelinedLoadAsync(5000, 100);

            Console.WriteLine("RunPipelined elapsed time is {0} for 4 ", sw.Elapsed);

            b.Stop();
        }
    }
}
