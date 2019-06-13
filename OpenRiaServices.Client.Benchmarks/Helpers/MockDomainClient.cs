using OpenRiaServices.DomainServices.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        protected override Task<QueryCompletedResult> QueryAsyncCore(EntityQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(QueryResult);
        }

        protected override InvokeCompletedResult EndInvokeCore(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        protected override Task<SubmitCompletedResult> SubmitAsyncCore(EntityChangeSet changeSet, CancellationToken cancellationToken)
        {
            return Task.FromResult(new SubmitCompletedResult(changeSet, changeSet.GetChangeSetEntries()));

        }
    }
}
