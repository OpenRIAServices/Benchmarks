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
    [ShortRunJob]
    public class EntityRefBenchmarks
    {
        [Params(10, 100, 1_000, 10_000, 100_000)]
        public int NumEntities { get; set; } = 1_000;

        [Benchmark]
        public void ColdLookup()
        {
            var (_, _, cities) = CreateLookupGraph(NumEntities, warmCache: false);

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
        public void CachedLookup()
        {
            var (_, _, cities) = CreateLookupGraph(NumEntities, warmCache: true);

            foreach (var city in cities)
            {
                var county = city.County;
                if (county == null)
                {
                    throw new InvalidOperationException("Expected matching County for the cached lookup case.");
                }
            }
        }

        [Benchmark]
        public void MissingKeyLookup()
        {
            var cities = CreateMissingKeyCities(NumEntities);

            foreach (var city in cities)
            {
                var county = city.County;
                if (county != null)
                {
                    throw new InvalidOperationException("Expected missing-key lookup to return null.");
                }
            }
        }

        [Benchmark]
        public void ForeignKeyChange()
        {
            var container = new EntitySetHelper.DynamicEntityContainer();
            var counties = container.AddEntitySet<County>(EntitySetOperations.All);
            var cities = container.AddEntitySet<City>(EntitySetOperations.All);

            var oldCounty = new County { Name = "OldCounty", StateName = "ST" };
            var newCounty = new County { Name = "NewCounty", StateName = "ST" };
            counties.Add(oldCounty);
            counties.Add(newCounty);

            foreach (var i in EnumerableRange(NumEntities))
            {
                var city = new City
                {
                    Name = $"FkCity-{i}",
                    CountyName = oldCounty.Name,
                    StateName = oldCounty.StateName,
                };

                cities.Add(city);
                city.County = oldCounty;

                city.CountyName = newCounty.Name;
                city.StateName = newCounty.StateName;

                var resolved = city.County;
                if (resolved != newCounty)
                {
                    throw new InvalidOperationException("Expected changed FK to resolve to the replacement County.");
                }
            }
        }

        [Benchmark]
        public void CompositeKeyLookup()
        {
            var (_, _, cities) = CreateLookupGraph(NumEntities, warmCache: false);

            foreach (var city in cities)
            {
                var county = city.County;
                if (county == null || county.Name != city.CountyName || county.StateName != city.StateName)
                {
                    throw new InvalidOperationException("Expected composite-key lookup to resolve the matching County.");
                }
            }
        }

        [Benchmark]
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

        private static (EntitySet<County> Counties, EntitySet<City> Cities, List<City> LookupCities) CreateLookupGraph(int count, bool warmCache)
        {
            var container = new EntitySetHelper.DynamicEntityContainer();
            var counties = container.AddEntitySet<County>(EntitySetOperations.All);
            var cities = container.AddEntitySet<City>(EntitySetOperations.All);
            var lookupCities = new List<City>(count);

            for (var i = 0; i < count; i++)
            {
                var county = new County { Name = $"County-{i}", StateName = "ST" };
                counties.Add(county);

                var city = new City
                {
                    Name = $"City-{i}",
                    CountyName = county.Name,
                    StateName = county.StateName,
                };

                cities.Add(city);
                if (warmCache)
                {
                    city.County = county;
                }

                lookupCities.Add(city);
            }

            return (counties, cities, lookupCities);
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
