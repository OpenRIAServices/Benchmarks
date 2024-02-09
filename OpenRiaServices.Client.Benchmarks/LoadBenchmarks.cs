using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using OpenRiaServices.Client.Benchmarks.Client.Cities;
using ClientBenchmarks.Helpers;
using OpenRiaServices.Client;
using OpenRiaServices.Client.Benchmarks.Server.Cities;
using City = OpenRiaServices.Client.Benchmarks.Client.Cities.City;

namespace ClientBenchmarks
{
    [MemoryDiagnoser]
    public class LoadBenchmarks
    {
        [Params(/*100, */10_000, 80_000)]
        public int NumEntities { get; set; } = 500;

        public LoadBenchmarks()
        {
            SynchronizationContext.SetSynchronizationContext(new NoOpSynchronizationContext());
        }


        static string ToAlphaKey(int num)
        {
            var sb = new StringBuilder();
            do
            {
                var alpha = (char)('a' + (num % 25));
                sb.Append(alpha);
                num /= 25;
            } while (num > 0);

            return sb.ToString();
        }

        private static List<City> CreateValidCities(int num)
        {
            List<City> cities = new List<City>(num);
            for (var i = 0; i < num; i++)
            {
                cities.Add(new City { Name = "Name" + ToAlphaKey(i), CountyName = "Country", StateName = "SA" }
                );
            }
            return cities;
        }

        MockDomainClient _mockDomainClient;
        CityDomainContext _ctx;
        List<City> _cities1;
        List<City> _cities2;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _mockDomainClient = new MockDomainClient();
            _ctx = new CityDomainContext(_mockDomainClient);
            _cities1 = CreateValidCities(NumEntities);
            _cities2 = CreateValidCities(NumEntities);
        }

        public void IterationSetup()
        {
            _ctx.EntityContainer.Clear();
        }

        [Benchmark]
        public void LoadEntities()
        {
            IterationSetup();

            _mockDomainClient.SetQueryResult(_cities1);
            var res = _ctx.Load(_ctx.GetCitiesQuery(), true);
            if (res.HasError)
                throw new Exception("Operation should not have had erros");
            if (!res.IsComplete)
                throw new Exception("Operation should have completed");
        }

        [Benchmark]
        public void LoadAndMergeEntities()
        {
            IterationSetup();

            _mockDomainClient.SetQueryResult(_cities1);
            var res = _ctx.Load(_ctx.GetCitiesQuery(), true);
            if (res.HasError)
                throw new Exception("Operation should not have had erros");
            if (!res.IsComplete)
                throw new Exception("Operation should have completed");

            _mockDomainClient.SetQueryResult(_cities2);
            res = _ctx.Load(_ctx.GetCitiesQuery(), LoadBehavior.MergeIntoCurrent, true);
            if (res.HasError)
                throw new Exception("Operation should not have had erros");
            if (!res.IsComplete)
                throw new Exception("Operation should have completed");
        }

        [Benchmark]
        public void LoadAndRefreshEntities()
        {
            IterationSetup();

            _mockDomainClient.SetQueryResult(_cities1);
            var res = _ctx.Load(_ctx.GetCitiesQuery(), true);
            if (res.HasError)
                throw new Exception("Operation should not have had erros");
            if (!res.IsComplete)
                throw new Exception("Operation should have completed");

            _mockDomainClient.SetQueryResult(_cities2);
            res = _ctx.Load(_ctx.GetCitiesQuery(), LoadBehavior.RefreshCurrent, true);
            if (res.HasError)
                throw new Exception("Operation should not have had erros");
            if (!res.IsComplete)
                throw new Exception("Operation should have completed");
        }
    }
}
