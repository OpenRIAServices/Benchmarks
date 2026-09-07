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
using ClientBenchmarks.Server.Example;
using OpenRiaServices.Hosting.AspNetCore.Serialization;
using System.Buffers;
using Microsoft.AspNetCore.Hosting;
using System.Runtime.Versioning;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

#if NET48
using System.Configuration;
using System.ServiceModel.Activation;

using server::OpenRiaServices.Hosting.Wcf;
using server::OpenRiaServices.Hosting.Wcf.Configuration;
#else
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenRiaServices.Hosting.AspNetCore;
using System.Text;
#endif

namespace ClientBenchmarks
{
  

    internal class Program
    {
        static bool onlyProfiling = false;

        //[STAThread]
        private static void Main(string[] args)
        {
            if (args.Length > 0 && args.Contains("server"))
            {

                Console.WriteLine($"Runtime {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");

                Uri uri = new Uri($"http://localhost/Temporary_Listen_Addresses/Cities/Services/Citites.svc");
                CityDomainService.GetCitiesResult = E2Ebenchmarks.CreateValidCities(1).ToList();
                StartServerAndWaitForKey(uri, typeof(CityDomainService));
                return;
            }

            if (onlyProfiling || (args.Length > 0 && args[0].Contains("profile")))
            {
                var es = new EntitySetBenchmarks();
                es.NumEntities = 50_000;
                es.GlobalSetup();

                Console.WriteLine("Warmup ..");
                es.Add();
                es.AddAndDetach();
                es.AddAndRemove();
                es.Attach();
                es.AttachAndDetach();
                es.AttachAndModify();
                es.AttachAndRemove();

                Console.WriteLine("Sleeping");
                Thread.Sleep(5000);

                es = new EntitySetBenchmarks();
                es.NumEntities = 50_000;
                es.GlobalSetup();

                for (int i = 0; i < 10; ++i)
                {
                    es.Add();
                    es.AddAndDetach();
                    es.AddAndRemove();
                    es.Attach();
                    es.AttachAndDetach();
                    es.AttachAndModify();
                    es.AttachAndRemove();
                }

                Console.WriteLine("Done..");
                Thread.Sleep(5000);
                return;
                //Task.Run(() => RunBenchmarksAsyncParallel()).Wait();

                //Task.Run(() => RunPipelined()).Wait();
                //Console.WriteLine("PAUSE");
                //Console.ReadLine();
                //return;

                //const int num = 1;
                //for (int i = 0; i < 1; ++i)
                //{
                //    var a = new LoadBenchmarks();
                //    a.NumEntities = num;
                //    a.LoadEntities();
                //    a.LoadAndRefreshEntities();
                //    a.LoadAndMergeEntities();

                //    var s = new ChangeSetBenchmarks();
                //    s.NumEntities = num;
                //    s.SubmitAdded();

                //    var es = new EntitySetBenchmarks();
                //    es.NumEntities = num;
                //es.Add();
                //es.AddAndDetach();
                //es.AddAndRemove();
                //es.Attach();
                //es.AttachAndDetach();
                //es.AttachAndModify();
                //es.AttachAndRemove();
                //}
            }
            else
            {
                //BenchmarkRunner.Run<MemCopyBenchmarks>(null, args);
                //BenchmarkRunner.Run<EntitySetBenchmarks>();
                BenchmarkRunner.Run<LoadBenchmarks>();
                //BenchmarkRunner.Run<E2Ebenchmarks>();
                //BenchmarkRunner.Run<EntityBenchmarks>();
                //BenchmarkRunner.Run<EntitySetBenchmarks>();
                //BenchmarkRunner.Run<ChangeSetBenchmarks>();
            }

        }


        private static void StartServerAndWaitForKey(Uri uri, Type type)
        {
#if NET48
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
#else
            const bool UseResponseCompression = false;
            var builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs());
            builder.WebHost.UseUrls(new[] { "https://localhost:7045", "http://localhost:5000" });

            // Register AddOpenRiaServices and all DomainServices
            builder.Services.AddOpenRiaServices()
                .AddMessagePackSerialization(true);
            builder.Services.AddTransient(type);

            // Add compression support so that we can support brotli compression (and don't have to rely on IIS gzip)
            if (UseResponseCompression)
                builder.Services.AddResponseCompression(options =>
                {
                    // https://docs.microsoft.com/en-us/aspnet/core/performance/response-compression?view=aspnetcore-6.0#risk
                    options.EnableForHttps = true;
                    options.MimeTypes = new[] { "application/msbin1" };
                });

            var app = builder.Build();

            if (UseResponseCompression)
                app.UseResponseCompression();
            // Enable mapping of all requests to root 
            app.MapOpenRiaServices(builder =>
            {
                builder.AddDomainService(type);
            });

            // new ArrayPoolStream.BufferMemory(ArrayPool<byte>.Shared, )

            app.MapGet("/", httpContext =>
            {
                var dataSource = httpContext.RequestServices.GetRequiredService<EndpointDataSource>();

                var sb = new StringBuilder();
                sb.Append("<html><body>");
                sb.AppendLine("<p>Endpoints:</p>");
                foreach (var endpoint in dataSource.Endpoints.OfType<RouteEndpoint>().OrderBy(e => e.RoutePattern.RawText, StringComparer.OrdinalIgnoreCase))
                {
                    sb.AppendLine(FormattableString.Invariant($"- <a href=\"{endpoint.RoutePattern.RawText}\">{endpoint.RoutePattern.RawText}</a><br />"));
                    foreach (var metadata in endpoint.Metadata)
                    {
                        sb.AppendLine("<li>" + metadata + "</li>");
                    }
                }

                var response = httpContext.Response;
                response.StatusCode = 200;

                sb.AppendLine("</body></html>");
                response.ContentType = "text/html";
                return response.WriteAsync(sb.ToString());
            });

            app.Start();
            Console.WriteLine($"DomainService {type.Name} running");
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
            app.StopAsync().GetAwaiter().GetResult();
#endif
        }

        private static ArrayPoolStream.BufferMemory WriteMemory(byte[] megabyte, int size)
        {
            var stream = new ArrayPoolStream(ArrayPool<byte>.Shared, maxBlockSize: 4 * 1024 * 1024);
            stream.Reset(4096);
            for (int i = 0; i < size; ++i)
                stream.Write(megabyte);

            var memory = stream.GetBufferMemoryAndReset();
            return memory;
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
