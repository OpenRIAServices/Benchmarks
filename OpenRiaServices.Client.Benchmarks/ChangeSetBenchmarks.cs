using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using Cities;
using ClientBenchmarks.Helpers;
using OpenRiaServices.DomainServices.Client;

namespace ClientBenchmarks
{
    [MemoryDiagnoser]
    [LegacyJitX86Job, RyuJitX64Job]
    public class ChangeSetBenchmarks
    {
        [Params(10, 100, 1000)]
        public int NumEntities { get; set; } = 500;

        public ChangeSetBenchmarks()
        {
            SynchronizationContext.SetSynchronizationContext(new NoOpSynchronizationContext());
        }


        private static IEnumerable<City> CreateCities(int num)
        {
            for (var i = 0; i < num; i++)
            {
                yield return new City { Name = "" + i, CountyName = "c", StateName = "s" };
            }
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

        private static IEnumerable<City> CreateValidCities(int num)
        {
            for (var i = 0; i < num; i++)
            {
                yield return new City { Name = "Name" + ToAlphaKey(i), CountyName = "Country", StateName = "SA" };
            }
        }

        [Benchmark]
        public void SubmitAddedWithValidationErrors()
        {
            var ctx = new CityDomainContext(new MockDomainClient());

            foreach (var city in CreateCities(NumEntities))
                ctx.Cities.Add(city);

            try
            {
                var res = ctx.SubmitChanges();
                if (!res.HasError)
                    throw new Exception("Operation should have had erros");
                if (res.IsComplete)
                    throw new Exception("Operation should have completed");
            }
            catch (SubmitOperationException)
            {


            }
        }

        [Benchmark]
        public void SubmitAdded()
        {
            var ctx = new CityDomainContext(new MockDomainClient());

            foreach (var city in CreateValidCities(NumEntities))
                ctx.Cities.Add(city);

            var res = ctx.SubmitChanges();

            if (!res.IsComplete)
                throw new Exception("Operation should have completed");
        }
    }
}
