extern alias server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using OpenRiaServices.Client.Benchmarks.Client.Cities;
using OpenRiaServices.Client.Benchmarks.Server.Cities;
using OpenRiaServices.DomainServices.Client;
using OpenRiaServices.DomainServices.Client.Web;
using server::OpenRiaServices.DomainServices.Hosting;
using server::OpenRiaServices.DomainServices.Server;

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
    //[LegacyJitX86Job, RyuJitX64Job]
    public class E2Ebenchmarks
    {
        const int _port = 60001;
        Uri _uri = new Uri($"http://localhost/Temporary_Listen_Addresses/Cities/Services/Citites.svc");
        DomainServiceHost _host;
        CityDomainContext _ctx;

        [Params(/*10,*/ 100, 1000)]
        public int NumEntities { get; set; } = 500;

        //[Params(DomainClientType.WcfBinary, DomainClientType.HttpBinary)]
        [Params(DomainClientType.HttpBinary, DomainClientType.WcfBinary)]
        public DomainClientType DomainClient { get; set;}

        [IterationSetup]
        public void Start()
        {
            
            _host = new DomainServiceHost(typeof(CityDomainService), _uri);
            //other relevent code to configure host's end point etc
            if (_host.Description.Behaviors.Contains(typeof(AspNetCompatibilityRequirementsAttribute)))
            {
                var compatibilityRequirementsAttribute = _host.Description.Behaviors[typeof(AspNetCompatibilityRequirementsAttribute)] as AspNetCompatibilityRequirementsAttribute;
                compatibilityRequirementsAttribute.RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed;
            }

            _host.Open();

            switch(DomainClient)
            {
                case DomainClientType.HttpBinary:
                    DomainContext.DomainClientFactory = new OpenRiaServices.DomainServices.Client.PortableWeb.WebApiDomainClientFactory();
                    break;
                case DomainClientType.HttpBinaryWinHttp:
                    DomainContext.DomainClientFactory = new OpenRiaServices.DomainServices.Client.PortableWeb.WebApiDomainClientFactory()
                    {
                         HttpClientHandler = new WinHttpHandler() {}
                    };
                    break;
                case DomainClientType.WcfBinary:
                    DomainContext.DomainClientFactory = new OpenRiaServices.DomainServices.Client.Web.WebDomainClientFactory();
                    break;
                default:
                    throw new NotImplementedException();
            }

            _ctx = new CityDomainContext(_uri);

            CityDomainService.GetCitiesResult = CreateValidCities(NumEntities).ToList();
        }

        public static IEnumerable<OpenRiaServices.Client.Benchmarks.Server.Cities.City> CreateValidCities(int num)
        {
            for (var i = 0; i < num; i++)
            {
                yield return new OpenRiaServices.Client.Benchmarks.Server.Cities.City { Name = "Name" + ChangeSetBenchmarks.ToAlphaKey(i), CountyName = "Country", StateName = "SA" };
            }
        }

        [IterationCleanup]
        public void Stop()
        {
            _host.Close();
        }

        [Benchmark]
        public async Task<LoadResult<OpenRiaServices.Client.Benchmarks.Client.Cities.City>> GetCititesUniqueContext()
        {
            CityDomainContext ctx = new CityDomainContext(_uri);
            return await ctx.LoadAsync(ctx.GetCitiesQuery()).ConfigureAwait(false);
        }

        [Benchmark]
        public async Task<LoadResult<OpenRiaServices.Client.Benchmarks.Client.Cities.City>> GetCititesReuseContext()
        {
            return await _ctx.LoadAsync(_ctx.GetCitiesQuery()).ConfigureAwait(false);
        }

        [Benchmark]
        public async Task Submit()
        {
            CityDomainContext ctx = new CityDomainContext(_uri);
            foreach (var city in ChangeSetBenchmarks.CreateValidCities(NumEntities))
                ctx.Cities.Add(city);

            var res = await ctx.SubmitChangesAsync().ConfigureAwait(false);

            if (res.ChangeSet.AddedEntities.Count != NumEntities)
                throw new Exception("Operation should have completed");
        }
    }
}
