using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using ClientBenchmarks.Helpers;
using OpenRiaServices.Client;
using OpenRiaServices.Client.Benchmarks.Client.Cities;

namespace ClientBenchmarks
{
    [MemoryDiagnoser]
//    [ShortRunJob]
    public class EntityRefBenchmarks
    {
        [Params(10, 500,/* 1_000,*/ 10_000/*, 100_000*/)]
        public int NumEntities { get; set; } = 1_000;


        [Benchmark]
        public void LookupExisting_CompositeKey()
        {
            var (_, _, cities) = CreateLookupGraph(NumEntities, addCitiesToSet: true);

            foreach (var city in cities)
            {
                var county = city.County;
                if (county == null)
                {
                    throw new InvalidOperationException("Expected matching County for the cold lookup case.");
                }
            }
        }

        [Benchmark]
        public void LookupExisting_StringKey()
        {
            var (counties, _, _) = CreateLookupGraph(NumEntities, addCitiesToSet: true);

            foreach (var county in counties)
            {
                var state = county.State;
                if (state == null)
                {
                    throw new InvalidOperationException("Expected matching State for the cold lookup case.");
                }
            }
        }

        [Benchmark]
        public void MissingKeyLookup_CompositeKey()
        {
            var cities = CreateMissingKeyCities(NumEntities);

            foreach (var city in cities)
            {
                city.CountyName = $"NotFound";
                var county = city.County;
                if (county != null)
                {
                    throw new InvalidOperationException("Expected missing-key lookup to return null.");
                }
            }
        }

        [Benchmark]
        public void MissingLookupExisting_StringKey()
        {
            var (counties, _, _) = CreateLookupGraph(NumEntities, addCitiesToSet: true);

            foreach (var county in counties)
            {
                county.StateName = "missing";
                var state = county.State;
                if (state != null)
                {
                    throw new InvalidOperationException("Expected missing-key lookup to return null.");
                }
            }
        }

        [Benchmark]
        public void ForeignKeyChange_StringKey()
        {
            var (counties, _, _) = CreateLookupGraph(NumEntities, addCitiesToSet: true);

            int idx = 1;
            foreach (var county in counties)
            {
                // Change the FK to the next item in the list, wrapping around to the first item.
                county.StateName = $"ST{idx++ % NumEntities}";
                var state = county.State;
                if (state == null || state.Name != county.StateName)
                {
                    throw new InvalidOperationException("Expected missing-key lookup to return null.");
                }
            }
        }

        //[Benchmark]
        public void AttachAndDetach()
        {
            for (var i = 0; i < NumEntities; i++)
            {
                var container = new EntitySetHelper.DynamicEntityContainer();
                var counties = container.AddEntitySet<County>(EntitySetOperations.All);
                var cities = container.AddEntitySet<City>(EntitySetOperations.All);

                var county = new County { Name = $"AttachCounty-{i}", StateName = "ST" };
                counties.Add(county);

                var city = new City
                {
                    Name = $"AttachCity-{i}",
                    CountyName = county.Name,
                    StateName = county.StateName,
                };

                cities.Attach(city);
                city.County = county;
                cities.Detach(city);
            }
        }

        private static (EntitySet<County> Counties, EntitySet<City> Cities, List<City> LookupCities) CreateLookupGraph(int count, bool addCitiesToSet)
        {
            var container = new EntitySetHelper.DynamicEntityContainer();
            var counties = container.AddEntitySet<County>(EntitySetOperations.All);
            var cities = container.AddEntitySet<City>(EntitySetOperations.All);
            var states = container.AddEntitySet<State>(EntitySetOperations.All);
            var citiesList = new List<City>(count);

            for (var i = 0; i < count; i++)
            {
                var county = new County { Name = $"County-{i}", StateName = $"ST{i}" };
                counties.Attach(county);

                states.Attach(new State() { Name = county.StateName, FullName="Full name" });

                var city = new City
                {
                    Name = $"City-{i}",
                    CountyName = county.Name,
                    StateName = county.StateName,
                };

                citiesList.Add(city);
                if (addCitiesToSet)
                {
                    cities.Attach(city);
                }
            }

            return (counties, cities, citiesList);
        }

        private static List<City> CreateMissingKeyCities(int count)
        {
            var container = new EntitySetHelper.DynamicEntityContainer();
            var counties = container.AddEntitySet<County>(EntitySetOperations.All);
            var cities = container.AddEntitySet<City>(EntitySetOperations.All);

            for (var i = 0; i < count; i++)
            {
                var county = new County { Name = $"ExistingCounty-{i}", StateName = "ST" };
                counties.Add(county);

                var city = new City
                {
                    Name = $"MissingCity-{i}",
                    CountyName = $"NotFound-{i}",
                    StateName = "XX",
                };

                cities.Add(city);
            }

            return cities.ToList();
        }

        private static IEnumerable<int> EnumerableRange(int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return i;
            }
        }
    }
}
