using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        private static IEnumerable<City> CreateValidCities(int num)
        {
            for (var i = 0; i < num; i++)
            {
                yield return new City { Name = "Name" + ToAlphaKey(i), CountyName = "Country", StateName = "SA" };
            }
        }

        [Benchmark]
        public void LoadEntities()
        {
            var mockDomainClient = new MockDomainClient();
            var ctx = new CityDomainContext(mockDomainClient);

            try
            {

                var res = ctx.Load(ctx.GetCitiesQuery(), true);
                if (res.HasError)
                    throw new Exception("Operation should have had erros");
                if (res.IsComplete)
                    throw new Exception("Operation should have completed");
            }
            catch (DomainOperationException)
            {


            }
        }


        public class MockDomainClient : DomainClient
        {
            protected override IAsyncResult BeginQueryCore(EntityQuery query, AsyncCallback callback, object userState)
            {
                var cities = LoadBenchmarks.CreateValidCities(500);
                var result = new MockLoadResult
                {
                    QueryResult = new QueryCompletedResult(cities, Enumerable.Empty<Entity>(), 1000, Enumerable.Empty<ValidationResult>()),
                    AsyncState = userState,
                };

                callback(result);
                return result;
            }

            protected override InvokeCompletedResult EndInvokeCore(IAsyncResult asyncResult)
            {
                throw new NotImplementedException();
            }

            protected override QueryCompletedResult EndQueryCore(IAsyncResult asyncResult)
            {
                var loadResult = ((MockLoadResult)(asyncResult));
                return loadResult.QueryResult;
            }

            protected override SubmitCompletedResult EndSubmitCore(IAsyncResult asyncResult)
            {
                var submitResult = ((MockSubmitResult)(asyncResult));
                return new SubmitCompletedResult(submitResult.EntityChangeSet, submitResult.ChangeSetEntries); ;
            }

            protected override IAsyncResult BeginSubmitCore(EntityChangeSet changeSet, AsyncCallback callback, object userState)
            {
                var asyncResult = new MockSubmitResult()
                {
                    AsyncState = userState,
                    EntityChangeSet = changeSet,
                    ChangeSetEntries = changeSet.GetChangeSetEntries(),
                };
                callback(asyncResult);
                return asyncResult;
            }

            class MockLoadResult : IAsyncResult
            {
                public QueryCompletedResult QueryResult { get; set; }

                public object AsyncState { get; set; }

                public WaitHandle AsyncWaitHandle => null;

                public bool CompletedSynchronously => true;

                public bool IsCompleted => true;
            }

            class MockSubmitResult : IAsyncResult
            {
                public EntityChangeSet EntityChangeSet { get; set; }

                public IEnumerable<ChangeSetEntry> ChangeSetEntries { get; set; }

                public object AsyncState { get; set; }

                public WaitHandle AsyncWaitHandle => null;

                public bool CompletedSynchronously => true;

                public bool IsCompleted => true;
            }
        }
    }


}
