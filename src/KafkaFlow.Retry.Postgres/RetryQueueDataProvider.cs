using KafkaFlow.Retry.Durable.Repository.Actions.Delete;

namespace KafkaFlow.Retry.Postgres
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Dawn;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Repository.Actions.Create;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.Postgres.Model;
    using KafkaFlow.Retry.Postgres.Model.Factories;
    using KafkaFlow.Retry.Postgres.Readers;
    using KafkaFlow.Retry.Postgres.Repositories;
    
    internal sealed class RetryQueueDataProvider : IRetryDurableQueueRepositoryProvider
    {
        private readonly IConnectionProvider connectionProvider;
        private readonly IRetryQueueDboFactory retryQueueDboFactory;
        private readonly IRetryQueueItemDboFactory retryQueueItemDboFactory;
        private readonly IRetryQueueItemMessageDboFactory retryQueueItemMessageDboFactory;
        private readonly IRetryQueueItemMessageHeaderDboFactory retryQueueItemMessageHeaderDboFactory;
        private readonly IRetryQueueItemMessageHeaderRepository retryQueueItemMessageHeaderRepository;
        private readonly IRetryQueueItemMessageRepository retryQueueItemMessageRepository;
        private readonly IRetryQueueItemRepository retryQueueItemRepository;
        private readonly IRetryQueueReader retryQueueReader;
        private readonly IRetryQueueRepository retryQueueRepository;
        private readonly PostgresDbSettings postgresDbSettings;

        public RetryQueueDataProvider(
            PostgresDbSettings postgresDbSettings,
            IConnectionProvider connectionProvider,
            IRetryQueueItemMessageHeaderRepository retryQueueItemMessageHeaderRepository,
            IRetryQueueItemMessageRepository retryQueueItemMessageRepository,
            IRetryQueueItemRepository retryQueueItemRepository,
            IRetryQueueRepository retryQueueRepository,
            IRetryQueueDboFactory retryQueueDboFactory,
            IRetryQueueItemDboFactory retryQueueItemDboFactory,
            IRetryQueueReader retryQueueReader,
            IRetryQueueItemMessageDboFactory retryQueueItemMessageDboFactory,
            IRetryQueueItemMessageHeaderDboFactory retryQueueItemMessageHeaderDboFactory)
        {
            this.postgresDbSettings = postgresDbSettings;
            this.connectionProvider = connectionProvider;
            this.retryQueueItemMessageHeaderRepository = retryQueueItemMessageHeaderRepository;
            this.retryQueueItemMessageRepository = retryQueueItemMessageRepository;
            this.retryQueueItemRepository = retryQueueItemRepository;
            this.retryQueueRepository = retryQueueRepository;
            this.retryQueueDboFactory = retryQueueDboFactory;
            this.retryQueueItemDboFactory = retryQueueItemDboFactory;
            this.retryQueueReader = retryQueueReader;
            this.retryQueueItemMessageDboFactory = retryQueueItemMessageDboFactory;
            this.retryQueueItemMessageHeaderDboFactory = retryQueueItemMessageHeaderDboFactory;
        }

        public async Task<CheckQueueResult> CheckQueueAsync(CheckQueueInput input)
        {
            Guard.Argument(input).NotNull();

            // Tries to find an active queue for the GroupKey
            using (var dbConnection = this.connectionProvider.Create(this.postgresDbSettings))
            {
                var exists = await this.retryQueueRepository.ExistsActiveAsync(dbConnection, input.QueueGroupKey).ConfigureAwait(false);

                return new CheckQueueResult(
                    exists ?
                        CheckQueueResultStatus.Exists :
                        CheckQueueResultStatus.DoesNotExist);
            }
        }

        public async Task<QueueNewestItemsResult> CheckQueueNewestItemsAsync(QueueNewestItemsInput input)
        {
            Guard.Argument(input, nameof(input)).NotNull();

            using (var dbConnection = this.connectionProvider.Create(this.postgresDbSettings))
            {
                var itemsDbo = await this.retryQueueItemRepository.GetNewestItemsAsync(dbConnection, input.QueueId, input.Sort).ConfigureAwait(false);

                if (itemsDbo.Any())
                {
                    return new QueueNewestItemsResult(QueueNewestItemsResultStatus.HasNewestItems);
                }

                return new QueueNewestItemsResult(QueueNewestItemsResultStatus.NoNewestItems);
            }
        }

        public async Task<QueuePendingItemsResult> CheckQueuePendingItemsAsync(QueuePendingItemsInput input)
        {
            Guard.Argument(input, nameof(input)).NotNull();

            using (var dbConnection = this.connectionProvider.Create(this.postgresDbSettings))
            {
                var itemsDbo = await this.retryQueueItemRepository.GetPendingItemsAsync(dbConnection, input.QueueId, input.Sort).ConfigureAwait(false);

                if (itemsDbo.Any())
                {
                    return new QueuePendingItemsResult(QueuePendingItemsResultStatus.HasPendingItems);
                }

                return new QueuePendingItemsResult(QueuePendingItemsResultStatus.NoPendingItems);
            }
        }

        public async Task<DeleteQueuesResult> DeleteQueuesAsync(DeleteQueuesInput input)
        {
            Guard.Argument(input, nameof(input)).NotNull();

            using (var dbConnection = this.connectionProvider.Create(this.postgresDbSettings))
            {
                var totalQueuesDeleted = await this.retryQueueRepository.DeleteQueuesAsync(
                        dbConnection,
                        input.SearchGroupKey,
                        input.RetryQueueStatus,
                        input.MaxLastExecutionDateToBeKept,
                        input.MaxRowsToDelete)
                    .ConfigureAwait(false);

                return new DeleteQueuesResult(totalQueuesDeleted);
            }
        }

        public async Task<GetQueuesResult> GetQueuesAsync(GetQueuesInput input)
        {
            Guard.Argument(input, nameof(input)).NotNull();

            RetryQueuesDboWrapper dboWrapper = new RetryQueuesDboWrapper();

            using (var dbConnection = this.connectionProvider.Create(this.postgresDbSettings))
            {
                dboWrapper.QueuesDbos = await this.retryQueueRepository.GetTopSortedQueuesOrderedAsync(
                    dbConnection,
                    input.Status,
                    input.SortOption,
                    input.SearchGroupKey,
                    input.TopQueues)
                    .ConfigureAwait(false);

                if (!dboWrapper.QueuesDbos.Any())
                {
                    return new GetQueuesResult(Enumerable.Empty<RetryQueue>());
                }

                var itemsDbo = new List<RetryQueueItemDbo>();

                var queueIds = dboWrapper.QueuesDbos.Select(q => q.IdDomain);

                foreach (var queueId in queueIds)
                {
                    var queueeItemsDbo = await this.retryQueueItemRepository.GetItemsOrderedAsync(
                        dbConnection,
                        new Guid[] { queueId },
                        input.ItemsStatuses,
                        input.SeverityLevels,
                        input.TopItemsByQueue,
                        input.StuckStatusFilter)
                        .ConfigureAwait(false);

                    itemsDbo.AddRange(queueeItemsDbo);
                }

                dboWrapper.ItemsDbos = itemsDbo;

                if (!dboWrapper.ItemsDbos.Any())
                {
                    return new GetQueuesResult(this.retryQueueReader.Read(dboWrapper));
                }

                dboWrapper.MessagesDbos = await this.retryQueueItemMessageRepository.GetMessagesOrderedAsync(dbConnection, dboWrapper.ItemsDbos).ConfigureAwait(false);
                dboWrapper.HeadersDbos = await this.retryQueueItemMessageHeaderRepository.GetOrderedAsync(dbConnection, dboWrapper.MessagesDbos).ConfigureAwait(false);
            }

            var queues = this.retryQueueReader.Read(dboWrapper);

            return new GetQueuesResult(queues);
        }

        public async Task<SaveToQueueResult> SaveToQueueAsync(SaveToQueueInput input)
        {
            Guard.Argument(input).NotNull();

            using (var dbConnection = this.connectionProvider.CreateWithinTransaction(this.postgresDbSettings))
            {
                var retryQueueDbo = await this.retryQueueRepository.GetQueueAsync(dbConnection, input.QueueGroupKey).ConfigureAwait(false);

                SaveToQueueResultStatus resultStatus;

                if (retryQueueDbo is null)
                {
                    await this.CreateItemIntoANewQueueAsync(dbConnection, input).ConfigureAwait(false);
                    resultStatus = SaveToQueueResultStatus.Created;
                }
                else
                {
                    await this.AddItemIntoAnExistingQueueAsync(dbConnection, input, retryQueueDbo).ConfigureAwait(false);
                    resultStatus = SaveToQueueResultStatus.Added;
                }

                dbConnection.Commit();

                return new SaveToQueueResult(resultStatus);
            }
        }

        public async Task<UpdateItemResult> UpdateItemExecutionInfoAsync(UpdateItemExecutionInfoInput input)
        {
            Guard.Argument(input, nameof(input)).NotNull();

            return await this.UpdateItemAndTryUpdateQueueToDoneAsync(input).ConfigureAwait(false);
        }

        public async Task<UpdateItemsResult> UpdateItemsAsync(UpdateItemsInput input)
        {
            Guard.Argument(input, nameof(input)).NotNull();

            var results = new List<UpdateItemResult>();

            using (var dbConnection = this.connectionProvider.Create(this.postgresDbSettings))
            {
                foreach (var itemId in input.ItemIds)
                {
                    var result = await this.UpdateItemAndQueueStatusAsync(new UpdateItemStatusInput(itemId, input.Status)).ConfigureAwait(false);

                    results.Add(result);
                }
            }

            return new UpdateItemsResult(results);
        }

        public async Task<UpdateItemResult> UpdateItemStatusAsync(UpdateItemStatusInput input)
        {
            Guard.Argument(input, nameof(input)).NotNull();

            using (var dbConnection = this.connectionProvider.Create(this.postgresDbSettings))
            {
                var totalItemsUpdated = await this.retryQueueItemRepository
                    .UpdateStatusAsync(dbConnection, input.ItemId, input.Status).ConfigureAwait(false);

                return new UpdateItemResult(
                    input.ItemId,
                    totalItemsUpdated == 0 ?
                        UpdateItemResultStatus.ItemNotFound :
                        UpdateItemResultStatus.Updated);
            }
        }

        public async Task<UpdateQueuesResult> UpdateQueuesAsync(UpdateQueuesInput input)
        {
            Guard.Argument(input, nameof(input)).NotNull();

            var results = new List<UpdateQueueResult>();

            foreach (var queueGroupKey in input.QueueGroupKeys)
            {
                var result = await this.UpdateQueueAndAllItemsAsync(new UpdateItemsInQueueInput(queueGroupKey, input.ItemStatus)).ConfigureAwait(false);

                results.Add(result);
            }

            return new UpdateQueuesResult(results);
        }

        private async Task AddItemAsync(IDbConnection dbConnection, SaveToQueueInput input, long retryQueueId, Guid retryQueueDomainId)
        {
            var retryQueueItemDbo = this.retryQueueItemDboFactory.Create(input, retryQueueId, retryQueueDomainId);

            var retryQueueItemId = await this.retryQueueItemRepository.AddAsync(dbConnection, retryQueueItemDbo).ConfigureAwait(false);

            // queue item message
            var retryQueueItemMessageDbo = this.retryQueueItemMessageDboFactory.Create(input.Message, retryQueueItemId);
            await this.retryQueueItemMessageRepository.AddAsync(dbConnection, retryQueueItemMessageDbo).ConfigureAwait(false);

            // queue item message header
            var retryQueueHeadersDbo = this.retryQueueItemMessageHeaderDboFactory.Create(input.Message.Headers, retryQueueItemId);
            await this.retryQueueItemMessageHeaderRepository.AddAsync(dbConnection, retryQueueHeadersDbo).ConfigureAwait(false);
        }

        private async Task AddItemIntoAnExistingQueueAsync(IDbConnection dbConnection, SaveToQueueInput input, RetryQueueDbo retryQueueDbo)
        {
            // Inserts the new item at the last position in the queue.

            // queue item
            await this.AddItemAsync(dbConnection, input, retryQueueDbo.Id, retryQueueDbo.IdDomain).ConfigureAwait(false);

            // Verifies whether to change the queue status.
            if (retryQueueDbo.Status == RetryQueueStatus.Done)
            {
                // The queue was marked as DONE. With this new item, the status should return to ACTIVE.
                await this.retryQueueRepository.UpdateStatusAsync(dbConnection, retryQueueDbo.IdDomain, RetryQueueStatus.Active).ConfigureAwait(false);
            }
        }

        private async Task CreateItemIntoANewQueueAsync(IDbConnection dbConnection, SaveToQueueInput input)
        {
            var retryQueueDbo = this.retryQueueDboFactory.Create(input);

            // queue
            var retryQueueId = await this.retryQueueRepository.AddAsync(dbConnection, retryQueueDbo).ConfigureAwait(false);

            // queue item
            await this.AddItemAsync(dbConnection, input, retryQueueId, retryQueueDbo.IdDomain).ConfigureAwait(false);
        }

        private bool IsItemInWaitingState(RetryQueueItemDbo item)
        {
            return item.Status == RetryQueueItemStatus.Waiting;
        }

        private async Task<UpdateQueueResultStatus> TryUpdateQueueToDoneAsync(IDbConnectionWithinTransaction dbConnection, Guid queueId)
        {
            var anyItemStillActive = await this.retryQueueItemRepository.AnyItemStillActiveAsync(dbConnection, queueId).ConfigureAwait(false);

            if (!anyItemStillActive)
            {
                var queueRowsAffected = await this.retryQueueRepository.UpdateStatusAsync(dbConnection, queueId, RetryQueueStatus.Done).ConfigureAwait(false);

                if (queueRowsAffected == 0)
                {
                    return UpdateQueueResultStatus.QueueNotFound;
                }

                return UpdateQueueResultStatus.Updated;
            }

            return UpdateQueueResultStatus.NotUpdated;
        }

        private async Task<UpdateItemResult> UpdateItemAndQueueStatusAsync(UpdateItemStatusInput input)
        {
            if (input.Status != RetryQueueItemStatus.Cancelled)
            {
                return new UpdateItemResult(input.ItemId, UpdateItemResultStatus.UpdateIsNotAllowed);
            }

            using (var dbConnection = this.connectionProvider.CreateWithinTransaction(this.postgresDbSettings))
            {
                var item = await this.retryQueueItemRepository.GetItemAsync(dbConnection, input.ItemId).ConfigureAwait(false);

                if (item is null)
                {
                    return new UpdateItemResult(input.ItemId, UpdateItemResultStatus.ItemNotFound);
                }

                if (!this.IsItemInWaitingState(item))
                {
                    return new UpdateItemResult(input.ItemId, UpdateItemResultStatus.ItemIsNotInWaitingState);
                }

                if (!await this.retryQueueItemRepository.IsFirstWaitingInQueueAsync(dbConnection, item).ConfigureAwait(false))
                {
                    return new UpdateItemResult(input.ItemId, UpdateItemResultStatus.ItemIsNotTheFirstWaitingInQueue);
                }

                var updateItemResult = await this.UpdateItemStatusAsync(input).ConfigureAwait(false);

                if (updateItemResult.Status == UpdateItemResultStatus.ItemNotFound)
                {
                    return updateItemResult;
                }

                var updateQueueResultStatus = await this.TryUpdateQueueToDoneAsync(dbConnection, item.DomainRetryQueueId).ConfigureAwait(false);

                if (updateQueueResultStatus == UpdateQueueResultStatus.QueueNotFound)
                {
                    return new UpdateItemResult(input.ItemId, UpdateItemResultStatus.QueueNotFound);
                }

                dbConnection.Commit();

                return new UpdateItemResult(input.ItemId, UpdateItemResultStatus.Updated);
            }
        }

        private async Task<UpdateItemResult> UpdateItemAndTryUpdateQueueToDoneAsync(UpdateItemExecutionInfoInput input)
        {
            using (var dbConnection = this.connectionProvider.CreateWithinTransaction(this.postgresDbSettings))
            {
                //update item
                var itemRowsAffected = await this.retryQueueItemRepository.UpdateAsync(dbConnection, input.ItemId, input.Status, input.AttemptCount, input.LastExecution, input.Description).ConfigureAwait(false);

                if (itemRowsAffected == 0)
                {
                    dbConnection.Commit();

                    return new UpdateItemResult(input.ItemId, UpdateItemResultStatus.ItemNotFound);
                }

                // update queue last execution and try update queue to done
                var updateQueueResultStatus = await this.UpdateQueueLastExecutionAndTryUpdateQueueToDoneAsync(dbConnection, input.QueueId, input.LastExecution).ConfigureAwait(false);

                if (updateQueueResultStatus == UpdateQueueResultStatus.QueueNotFound)
                {
                    return new UpdateItemResult(input.ItemId, UpdateItemResultStatus.QueueNotFound);
                }

                dbConnection.Commit();

                return new UpdateItemResult(input.ItemId, UpdateItemResultStatus.Updated);
            }
        }

        private async Task<UpdateQueueResult> UpdateQueueAndAllItemsAsync(UpdateItemsInQueueInput input)
        {
            using (var dbConnection = this.connectionProvider.CreateWithinTransaction(this.postgresDbSettings))
            {
                var queue = await this.retryQueueRepository.GetQueueAsync(dbConnection, input.QueueGroupKey).ConfigureAwait(false);

                if (queue is null)
                {
                    return new UpdateQueueResult(input.QueueGroupKey, UpdateQueueResultStatus.QueueNotFound, RetryQueueStatus.None);
                }

                if (input.ItemStatus != RetryQueueItemStatus.Cancelled)
                {
                    return new UpdateQueueResult(input.QueueGroupKey, UpdateQueueResultStatus.UpdateIsNotAllowed, queue.Status);
                }

                if (queue.Status != RetryQueueStatus.Active)
                {
                    return new UpdateQueueResult(input.QueueGroupKey, UpdateQueueResultStatus.QueueIsNotActive, queue.Status);
                }

                var items = await this.retryQueueItemRepository
                    .GetItemsOrderedAsync(dbConnection, new Guid[] { queue.IdDomain }, new RetryQueueItemStatus[] { RetryQueueItemStatus.Waiting })
                    .ConfigureAwait(false);

                if (!items.Any())
                {
                    return new UpdateQueueResult(input.QueueGroupKey, UpdateQueueResultStatus.QueueHasNoItemsWaiting, queue.Status);
                }

                foreach (var item in items)
                {
                    var updateItemResult = await this.UpdateItemStatusAsync(new UpdateItemStatusInput(item.IdDomain, input.ItemStatus)).ConfigureAwait(false);

                    if (updateItemResult.Status != UpdateItemResultStatus.Updated)
                    {
                        return new UpdateQueueResult(input.QueueGroupKey, UpdateQueueResultStatus.FailedToUpdateItems, queue.Status);
                    }
                }

                var updateQueueResultStatus = await this.TryUpdateQueueToDoneAsync(dbConnection, queue.IdDomain).ConfigureAwait(false);

                if (updateQueueResultStatus == UpdateQueueResultStatus.QueueNotFound)
                {
                    return new UpdateQueueResult(input.QueueGroupKey, UpdateQueueResultStatus.AllItemsUpdatedButFailedToUpdateQueue, queue.Status);
                }

                dbConnection.Commit();

                queue = await this.retryQueueRepository.GetQueueAsync(dbConnection, input.QueueGroupKey).ConfigureAwait(false);

                return new UpdateQueueResult(input.QueueGroupKey, updateQueueResultStatus, queue.Status);
            }
        }

        private async Task<UpdateQueueResultStatus> UpdateQueueLastExecutionAndTryUpdateQueueToDoneAsync(IDbConnectionWithinTransaction dbConnection, Guid queueId, DateTime lastExecution)
        {
            // check if the queue can be updated to done as well
            var anyItemStillActive = await this.retryQueueItemRepository.AnyItemStillActiveAsync(dbConnection, queueId).ConfigureAwait(false);

            if (anyItemStillActive)
            {
                // update queue last execution only
                var queueLastExecutionRowsAffected = await this.retryQueueRepository.UpdateLastExecutionAsync(dbConnection, queueId, lastExecution).ConfigureAwait(false);

                if (queueLastExecutionRowsAffected == 0)
                {
                    return UpdateQueueResultStatus.QueueNotFound;
                }
            }
            else
            {
                // update queue last execution and the status to done
                var queueRowsAffected = await this.retryQueueRepository.UpdateAsync(dbConnection, queueId, RetryQueueStatus.Done, lastExecution).ConfigureAwait(false);

                if (queueRowsAffected == 0)
                {
                    return UpdateQueueResultStatus.QueueNotFound;
                }
            }

            return UpdateQueueResultStatus.Updated;
        }
    }
}