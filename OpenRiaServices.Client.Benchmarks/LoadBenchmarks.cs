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

namespace ClientBenchmarks
{
    [MemoryDiagnoser]
    [LegacyJitX86Job, RyuJitX64Job]
    public class LoadBenchmarks
    {
        [Params(10, 100, 1000)]
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

        [Benchmark]
        public void NoOp()
        {
            var cities1 = CreateValidCities(NumEntities);
            var cities2 = CreateValidCities(NumEntities);

            var mockDomainClient = new MockDomainClient();
            var ctx = new CityDomainContext(mockDomainClient);

            mockDomainClient.SetQueryResult(cities1);
            mockDomainClient.SetQueryResult(cities2);
        }

        [Benchmark]
        public void LoadEntities()
        {
            var cities1 = CreateValidCities(NumEntities);
            var cities2 = CreateValidCities(NumEntities);

            var mockDomainClient = new MockDomainClient();
            var ctx = new CityDomainContext(mockDomainClient);

            mockDomainClient.SetQueryResult(cities1);
            var res = ctx.Load(ctx.GetCitiesQuery(), true);
            if (res.HasError)
                throw new Exception("Operation should not have had erros");
            if (!res.IsComplete)
                throw new Exception("Operation should have completed");
        }

        [Benchmark]
        public void LoadAndMergeEntities()
        {
            var cities1 = CreateValidCities(NumEntities);
            var cities2 = CreateValidCities(NumEntities);

            var mockDomainClient = new MockDomainClient();
            var ctx = new CityDomainContext(mockDomainClient);

            mockDomainClient.SetQueryResult(cities1);
            var res = ctx.Load(ctx.GetCitiesQuery(), true);
            if (res.HasError)
                throw new Exception("Operation should not have had erros");
            if (!res.IsComplete)
                throw new Exception("Operation should have completed");

            mockDomainClient.SetQueryResult(cities2);
            res = ctx.Load(ctx.GetCitiesQuery(), LoadBehavior.MergeIntoCurrent, true);
            if (res.HasError)
                throw new Exception("Operation should not have had erros");
            if (!res.IsComplete)
                throw new Exception("Operation should have completed");
        }

        [Benchmark]
        public void LoadAndRefreshEntities()
        {
            var cities1 = CreateValidCities(NumEntities);
            var cities2 = CreateValidCities(NumEntities);


            var mockDomainClient = new MockDomainClient();
            var ctx = new CityDomainContext(mockDomainClient);

            mockDomainClient.SetQueryResult(cities1);
            var res = ctx.Load(ctx.GetCitiesQuery(), true);
            if (res.HasError)
                throw new Exception("Operation should not have had erros");
            if (!res.IsComplete)
                throw new Exception("Operation should have completed");

            mockDomainClient.SetQueryResult(cities2);
            res = ctx.Load(ctx.GetCitiesQuery(), LoadBehavior.RefreshCurrent, true);
            if (res.HasError)
                throw new Exception("Operation should not have had erros");
            if (!res.IsComplete)
                throw new Exception("Operation should have completed");
        }
    }
}
