﻿extern alias server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using OpenRiaServices.Client.Benchmarks.Client.Cities;
using OpenRiaServices.Client.Benchmarks.Server.Cities;
using OpenRiaServices.Client;
using OpenRiaServices.Client.Web;

#if NET48
using System.ServiceModel.Activation;
using server::OpenRiaServices.Hosting.Wcf;
using server::OpenRiaServices.Server;
#endif

namespace ClientBenchmarks.Server
{
    public enum DomainClientType
    {
        WcfBinary,
        HttpBinary,
        HttpBinaryWinHttp,
    }

    public class Http2CustomHandler : WinHttpHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            request.Version = new Version("2.0");
            return base.SendAsync(request, cancellationToken);
        }
    }

    [MemoryDiagnoser]
    // [EtwProfiler]
    //[LegacyJitX86Job, RyuJitX64Job]
    // [ConcurrencyVisualizerProfiler]
    //[ShortRunJob]
    //[MediumRunJob]
    //[RyuJitX64Job]
    public class E2Ebenchmarks
    {
        Uri _uri = new Uri("http://localhost/Temporary_Listen_Addresses/Cities/Services/Citites.svc");
        //Uri _clientUri = new Uri("http://localhost.fiddler/Temporary_Listen_Addresses/Cities/Services/Citites.svc");
        Uri _clientUri = new Uri("http://localhost/Temporary_Listen_Addresses/Cities/Services/Citites.svc");
#if NET48
        DomainServiceHost _host;
#endif
        CityDomainContext _ctx;

        [Params(10, 100, 1000)]
        public int NumEntities { get; set; } = 500;

        //[Params(DomainClientType.WcfBinary, DomainClientType.HttpBinary)]
        [Params(DomainClientType.WcfBinary)]
        public DomainClientType DomainClient { get; set; }

        [GlobalSetup]

        public void Start()
        {
#if NET48

            _host = new DomainServiceHost(typeof(CityDomainService), _uri);

            //other relevent code to configure host's end point etc
            if (_host.Description.Behaviors.Contains(typeof(AspNetCompatibilityRequirementsAttribute)))
            {
                var compatibilityRequirementsAttribute = _host.Description.Behaviors[typeof(AspNetCompatibilityRequirementsAttribute)] as AspNetCompatibilityRequirementsAttribute;
                compatibilityRequirementsAttribute.RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed;
            }

            _host.Open();
#endif
            switch (DomainClient)
            {
                case DomainClientType.HttpBinary:
                    throw new NotImplementedException();
                    //DomainContext.DomainClientFactory = new OpenRiaServices.Client.PortableWeb.WebApiDomainClientFactory();
                    break;
                case DomainClientType.HttpBinaryWinHttp:
                    throw new NotImplementedException();
                    /*DomainContext.DomainClientFactory = new OpenRiaServices.Client.PortableWeb.WebApiDomainClientFactory()
                    {
                        HttpClientHandler = new WinHttpHandler() { }
                    };*/
                    break;
#if NET48
                case DomainClientType.WcfBinary:
                    DomainContext.DomainClientFactory = new OpenRiaServices.Client.Web.WebDomainClientFactory();
                    break;
#endif
                default:
                    throw new NotImplementedException();
            }

            _ctx = new CityDomainContext(_clientUri);
            CityDomainService.GetCitiesResult = CreateValidCities(NumEntities).ToList();

            _ctx.LoadAsync(_ctx.GetCitiesQuery()).GetAwaiter().GetResult();
        }

        public static IEnumerable<OpenRiaServices.Client.Benchmarks.Server.Cities.City> CreateValidCities(int num)
        {
            for (var i = 0; i < num; i++)
            {
                yield return new OpenRiaServices.Client.Benchmarks.Server.Cities.City { Name = "Name" + ChangeSetBenchmarks.ToAlphaKey(i), CountyName = "Country", StateName = "SA" };
            }
        }

        [GlobalCleanup]
        public void Stop()
        {
#if NET48
            _host.Close();
#endif
        }

        [Benchmark]
        public async Task<LoadResult<OpenRiaServices.Client.Benchmarks.Client.Cities.City>> GetCititesUniqueContext()
        {
            CityDomainContext ctx = new CityDomainContext(_clientUri);
            return await ctx.LoadAsync(ctx.GetCitiesQuery()).ConfigureAwait(false);
        }

        [Benchmark]
        public async Task<LoadResult<OpenRiaServices.Client.Benchmarks.Client.Cities.City>> GetCititesReuseContext()
        {
            return await _ctx.LoadAsync(_ctx.GetCitiesQuery()).ConfigureAwait(false);
        }

        [Benchmark]
        public Task<InvokeResult<string>> InvokeAsync()
        {
            return _ctx.EchoAsync("a");
        }

        const int ParallelInvokeIterations = 400;

        //[Arguments(ParallelInvokeIterations, 1)]
        //[Arguments(ParallelInvokeIterations, 2)]
        //[Arguments(ParallelInvokeIterations, 4)]
        //[Benchmark(OperationsPerInvoke = ParallelInvokeIterations)]
        public async Task RunBenchmarksAsyncParallel(int total = 1000, int concurrent = 8)
        {
            int outer = total / concurrent;
            var tasks = new Task<LoadResult<OpenRiaServices.Client.Benchmarks.Client.Cities.City>>[concurrent];

            for (int i = 0; i < outer; ++i)
            {
                for (int j = 0; j < concurrent; ++j)
                    tasks[j] = GetCititesReuseContext();

                var results = await Task.WhenAll(tasks);
            }
        }

        const int PipeLineInvocations = 400;

        //[Arguments(PipeLineInvocations, 1)]
        //[Arguments(PipeLineInvocations, 2)]
        //[Arguments(PipeLineInvocations, 4)]
        //[Benchmark(OperationsPerInvoke = PipeLineInvocations)]
        public async Task PipelinedLoadAsync(int total = 10, int depth = 2)
        {
            var tasks = new Task[depth];

            // start loading
            for (int i = 0; i < tasks.Length; ++i)
                tasks[i] = GetCititesReuseContext();

            int current = 0;
            for (int i = tasks.Length; i < total; ++i)
            {
                await tasks[current];
                tasks[current] = GetCititesReuseContext();
                current = (current + 1) % depth;
            }

            // Finish loading the rest
            for (int i = 0; i < tasks.Length; ++i)
                await tasks[i];
        }

        //    [Benchmark]
        public async Task Submit()
        {
            CityDomainContext ctx = new CityDomainContext(_clientUri);
            foreach (var city in ChangeSetBenchmarks.CreateValidCities(NumEntities))
                ctx.Cities.Add(city);

            var res = await ctx.SubmitChangesAsync().ConfigureAwait(false);

            if (res.ChangeSet.AddedEntities.Count != NumEntities)
                throw new Exception("Operation should have completed");
        }
    }
}
