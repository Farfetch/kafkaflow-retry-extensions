namespace KafkaFlow.Retry.MongoDb.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Dawn;
    using KafkaFlow.Retry.Durable.Common;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.MongoDb.Adapters;
    using KafkaFlow.Retry.MongoDb.Adapters.Interfaces;
    using KafkaFlow.Retry.MongoDb.Model;
    using KafkaFlow.Retry.MongoDb.Model.Factories;
    using MongoDB.Driver;

    internal class RetryQueueItemRepository : IRetryQueueItemRepository
    {
        private readonly DbContext dbContext;
        private readonly IQueuesAdapter queuesAdapter;
        private readonly RetryQueueItemDboFactory retryQueueItemDboFactory;

        public RetryQueueItemRepository(DbContext dbContext)
        {
            Guard.Argument(dbContext).NotNull();

            this.dbContext = dbContext;

            var messageAdapter = new MessageAdapter(new HeaderAdapter());

            this.retryQueueItemDboFactory = new RetryQueueItemDboFactory(messageAdapter);
            this.queuesAdapter = new QueuesAdapter(new ItemAdapter(messageAdapter));
        }

        public async Task<bool> AnyItemStillActiveAsync(Guid retryQueueId)
        {
            var itemsFilterBuilder = this.dbContext.RetryQueueItems.GetFilters();

            var itemsFilter = itemsFilterBuilder.Eq(i => i.RetryQueueId, retryQueueId)
                            & itemsFilterBuilder.Nin(i => i.Status, new RetryQueueItemStatus[] { RetryQueueItemStatus.Done, RetryQueueItemStatus.Cancelled });

            var itemsDbo = await this.dbContext.RetryQueueItems.GetAsync(itemsFilter).ConfigureAwait(false);

            return itemsDbo.Any();
        }

        public async Task<RetryQueueItemDbo> GetItemAsync(Guid itemId)
        {
            var queueItemsFilterBuilder = this.dbContext.RetryQueueItems.GetFilters();

            var queueItemsFilter = queueItemsFilterBuilder.Eq(q => q.Id, itemId);

            var items = await this.dbContext.RetryQueueItems.GetAsync(queueItemsFilter).ConfigureAwait(false);

            return items.FirstOrDefault();
        }

        public async Task<IEnumerable<RetryQueueItemDbo>> GetItemsAsync(
            IEnumerable<Guid> queueIds,
            IEnumerable<RetryQueueItemStatus> statuses,
            IEnumerable<SeverityLevel> severities = null,
            int? top = null,
            StuckStatusFilter stuckStatusFilter = null)
        {
            var itemsFilterBuilder = this.dbContext.RetryQueueItems.GetFilters();

            var itemsFilter = itemsFilterBuilder.In(i => i.RetryQueueId, queueIds);

            if (stuckStatusFilter is null)
            {
                itemsFilter &= itemsFilterBuilder.In(i => i.Status, statuses);
            }
            else
            {
                itemsFilter &= itemsFilterBuilder.Or(
                                itemsFilterBuilder.In(i => i.Status, statuses),
                                itemsFilterBuilder.Eq(i => i.Status, stuckStatusFilter.ItemStatus)
                                    & itemsFilterBuilder.Lt(i => i.ModifiedStatusDate, DateTime.UtcNow.AddSeconds(-stuckStatusFilter.ExpirationInterval.TotalSeconds)));
            }

            if (severities is object && severities.Any())
            {
                itemsFilter &= itemsFilterBuilder.In(i => i.SeverityLevel, severities);
            }

            var options = new FindOptions<RetryQueueItemDbo>
            {
                Sort = this.dbContext.RetryQueueItems.GetSortDefinition().Ascending(i => i.Sort),
                Limit = top
            };

            return await this.dbContext.RetryQueueItems.GetAsync(itemsFilter, options).ConfigureAwait(false);
        }

        public async Task<bool> IsFirstWaitingInQueue(RetryQueueItemDbo item)
        {
            var sortedItems = await this.GetItemsAsync(
                new Guid[] { item.RetryQueueId },
                new RetryQueueItemStatus[] { RetryQueueItemStatus.Waiting })
                .ConfigureAwait(false);

            if (sortedItems.Any() && item.Id == sortedItems.First().Id)
            {
                return true;
            }

            return false;
        }

        public async Task<UpdateItemResult> UpdateItemAsync(
            Guid itemId, RetryQueueItemStatus status, int attemptsCount, DateTime? lastExecution, string description)
        {
            var filter = this.dbContext.RetryQueueItems.GetFilters().Eq(i => i.Id, itemId);

            var update = this.dbContext.RetryQueueItems.GetUpdateDefinition()
                             .Set(i => i.Status, status)
                             .Set(i => i.AttemptsCount, attemptsCount)
                             .Set(i => i.LastExecution, lastExecution)
                             .Set(i => i.ModifiedStatusDate, DateTime.UtcNow);

            if (!string.IsNullOrEmpty(description))
            {
                update = update
                    .Set(i => i.Description, description);
            }

            var updateResult = await this.dbContext.RetryQueueItems.UpdateOneAsync(filter, update).ConfigureAwait(false);

            if (updateResult.IsAcknowledged && updateResult.MatchedCount == 0)
            {
                return new UpdateItemResult(itemId, UpdateItemResultStatus.ItemNotFound);
            }

            return new UpdateItemResult(itemId, UpdateItemResultStatus.Updated);
        }
    }
}