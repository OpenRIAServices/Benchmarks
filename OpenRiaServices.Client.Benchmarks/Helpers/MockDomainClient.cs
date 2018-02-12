using OpenRiaServices.DomainServices.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;

namespace ClientBenchmarks.Helpers
{
    public class MockDomainClient : DomainClient
    {
        /// <summary>
        /// The result of the next load
        /// </summary>
        public QueryCompletedResult QueryResult { get; set; }

        /// <summary>
        ///  Set the result of the next Load
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="relatedEntities"></param>
        /// <param name="totalCount"></param>
        /// <param name="validationResult"></param>
        public void SetQueryResult(IEnumerable<Entity> entities, IEnumerable<Entity> relatedEntities = null, int totalCount = -1, IEnumerable<ValidationResult> validationResult = null)
        {
            QueryResult = new QueryCompletedResult(entities.ToList(), 
                relatedEntities ?? Enumerable.Empty<Entity>(),
                totalCount,
                validationResult ?? Enumerable.Empty<ValidationResult>()
                );
        }

        protected override IAsyncResult BeginQueryCore(EntityQuery query, AsyncCallback callback, object userState)
        {
            var result = new MockLoadResult
            {
                QueryResult = this.QueryResult,
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
