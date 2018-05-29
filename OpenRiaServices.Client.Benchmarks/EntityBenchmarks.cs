using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using OpenRiaServices.Client.Benchmarks.Client.Cities;
using OpenRiaServices.DomainServices.Client;

namespace ClientBenchmarks
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class EntityBenchmarks //: EntitySetTests
    {
        private List<City> _cities;

        public int NumEntities { get; set; } = 500;

        [GlobalSetup]
        public void Setup()
        {
            _cities = CreateCities(NumEntities).ToList();
        }

        [Benchmark]
        public new void ToString()
        {
            foreach (var city in GetCities())
            {
                city.ToString();
            }
        }


        [Benchmark]
        public void GetIdentity()
        {
            foreach (var city in GetCities())
            {
                city.GetIdentity();
            }
        }

        private IEnumerable<City> GetCities() => _cities;

        private static IEnumerable<City> CreateCities(int num)
        {
            for (var i = 0; i < num; i++)
            {
                yield return new City { Name = "" + i, CountyName = "c", StateName = "s" };
            }
        }
    }
}
