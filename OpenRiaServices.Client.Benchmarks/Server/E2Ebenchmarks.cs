extern alias server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using OpenRiaServices.Client.Benchmarks.Client.Cities;
using OpenRiaServices.Client.Benchmarks.Server.Cities;
using OpenRiaServices.DomainServices.Client;
using OpenRiaServices.DomainServices.Client.Web;
using server::OpenRiaServices.DomainServices.Hosting;
using server::OpenRiaServices.DomainServices.Server;

namespace ClientBenchmarks.Server
{
    

    public class E2Ebenchmarks
    {
        const int _port = 60001;
        Uri _uri = new Uri($"http://localhost/Temporary_Listen_Addresses/Cities/Services/Citites.svc");
        DomainServiceHost _host;
        CityDomainContext _ctx;


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
            _ctx = new CityDomainContext(_uri);
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
            return await ctx.LoadAsync(ctx.GetCitiesQuery());
        }

        [Benchmark]
        public async Task<LoadResult<OpenRiaServices.Client.Benchmarks.Client.Cities.City>> GetCititesReuseContext()
        {
            return await _ctx.LoadAsync(_ctx.GetCitiesQuery());
        }
    }
}
