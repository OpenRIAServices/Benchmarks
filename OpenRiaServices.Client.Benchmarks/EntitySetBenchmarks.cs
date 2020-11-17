using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using OpenRiaServices.Client.Benchmarks.Client.Cities;
using OpenRiaServices.Client;


namespace ClientBenchmarks
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class EntitySetBenchmarks
    {
        private EntitySet<City> _entitySet;
        private List<City> _cities;

        [Params(10, 500, 5000)]
        public int NumEntities { get; set; } = 500;

        //[IterationSetup]
        public void Setup()
        {
            _entitySet = Helpers.EntitySetHelper.CreateEntitySet<City>();
            _cities = CreateCities(NumEntities).ToList();
        }

        [Benchmark]
        public void None()
        {
            Setup();
        }

        [Benchmark]
        public void Add()
        {
            Setup();

            foreach (var city in GetCities())
            {
                _entitySet.Add(city);
            }
        }


        [Benchmark]
        public void Attach()
        {
            Setup();

            foreach (var city in GetCities())
            {
                _entitySet.Attach(city);
            }
        }

        [Benchmark]
        public void AddAndModify()
        {
            Add();

            foreach (var entity in _entitySet.ToList())
            {
                entity.Name = "changed";
            }
        }

        [Benchmark]
        public void AddAndRemove()
        {
            Add();

            foreach (var entity in _entitySet.ToList())
            {
                _entitySet.Remove(entity);
            }
        }

        [Benchmark]
        public void AddAndDetach()
        {
            Add();

            foreach (var entity in _entitySet.ToList())
            {
                _entitySet.Detach(entity);
            }
        }

        [Benchmark]
        public void AttachAndModify()
        {
            Attach();

            foreach (var entity in _entitySet)
            {
                entity.Name = "changed";
            }
        }

        [Benchmark]
        public void AttachAndRemove()
        {
            Attach();

            foreach (var entity in _entitySet.ToList())
            {
                _entitySet.Remove(entity);
            }
        }

        [Benchmark]
        public void AttachAndDetach()
        {
            Attach();

            foreach (var entity in _entitySet.ToList())
            {
                _entitySet.Detach(entity);
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
