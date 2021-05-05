namespace KafkaFlow.Retry.MongoDb.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Dawn;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.MongoDb.Adapters;
    using KafkaFlow.Retry.MongoDb.Adapters.Interfaces;
    using KafkaFlow.Retry.MongoDb.Model;
    using KafkaFlow.Retry.MongoDb.Model.Factories;
    using MongoDB.Driver;

    internal class RetryQueueRepository : IRetryQueueRepository
    {
        private readonly DbContext dbContext;
        private readonly IQueuesAdapter queuesAdapter;
        private readonly RetryQueueItemDboFactory retryQueueItemDboFactory;

        public RetryQueueRepository(DbContext dbContext)
        {
            Guard.Argument(dbContext).NotNull();

            this.dbContext = dbContext;

            var messageAdapter = new MessageAdapter(new HeaderAdapter());

            this.retryQueueItemDboFactory = new RetryQueueItemDboFactory(messageAdapter);
            this.queuesAdapter = new QueuesAdapter(new ItemAdapter(messageAdapter));
        }

        public async Task<RetryQueueDbo> GetQueueAsync(string queueGroupKey)
        {
            var queuesFilterBuilder = this.dbContext.RetryQueues.GetFilters();

            var queuesFilter = queuesFilterBuilder.Eq(q => q.QueueGroupKey, queueGroupKey);

            return await this.dbContext.RetryQueues.GetOneAsync(queuesFilter).ConfigureAwait(false);
        }

        public async Task<IEnumerable<RetryQueueDbo>> GetTopSortedQueuesAsync(RetryQueueStatus status, GetQueuesSortOption sortOption, string searchGroupKey, int top)
        {
            var queuesFilterBuilder = this.dbContext.RetryQueues.GetFilters();

            var queuesFilter = queuesFilterBuilder.Eq(q => q.Status, status);

            if (searchGroupKey is object)
            {
                queuesFilter &= queuesFilterBuilder.Eq(q => q.SearchGroupKey, searchGroupKey);
            }

            SortDefinition<RetryQueueDbo> sortDefinition;

            switch (sortOption)
            {
                case GetQueuesSortOption.ByLastExecution_Ascending:
                    sortDefinition = this.dbContext.RetryQueues.GetSortDefinition().Ascending(i => i.LastExecution);
                    break;

                case GetQueuesSortOption.ByCreationDate_Descending:
                default:
                    sortDefinition = this.dbContext.RetryQueues.GetSortDefinition().Descending(i => i.CreationDate);
                    break;
            }

            var options = new FindOptions<RetryQueueDbo>
            {
                Sort = sortDefinition,
                Limit = top
            };

            return await this.dbContext.RetryQueues.GetAsync(queuesFilter, options).ConfigureAwait(false);
        }

        public async Task<UpdateResult> UpdateLastExecutionAsync(Guid queueId, DateTime lastExecution)
        {
            var filter = this.dbContext.RetryQueues.GetFilters().Eq(q => q.Id, queueId);

            var update = this.dbContext.RetryQueues.GetUpdateDefinition()
                             .Set(q => q.LastExecution, lastExecution);

            return await this.dbContext.RetryQueues.UpdateOneAsync(filter, update).ConfigureAwait(false);
        }

        public async Task<UpdateResult> UpdateStatusAndLastExecutionAsync(Guid queueId, RetryQueueStatus status, DateTime lastExecution)
        {
            var filter = this.dbContext.RetryQueues.GetFilters().Eq(q => q.Id, queueId);

            var update = this.dbContext.RetryQueues.GetUpdateDefinition()
                             .Set(q => q.Status, status)
                             .Set(q => q.LastExecution, lastExecution);

            return await this.dbContext.RetryQueues.UpdateOneAsync(filter, update).ConfigureAwait(false);
        }

        public async Task<UpdateResult> UpdateStatusAsync(Guid queueId, RetryQueueStatus status)
        {
            var filter = this.dbContext.RetryQueues.GetFilters().Eq(q => q.Id, queueId);

            var update = this.dbContext.RetryQueues.GetUpdateDefinition()
                             .Set(q => q.Status, status);

            return await this.dbContext.RetryQueues.UpdateOneAsync(filter, update).ConfigureAwait(false);
        }
    }
}