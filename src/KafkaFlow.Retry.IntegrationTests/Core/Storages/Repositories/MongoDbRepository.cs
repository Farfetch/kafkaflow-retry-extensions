namespace KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Models;
    using KafkaFlow.Retry.MongoDb.Model;
    using MongoDB.Driver;

    internal class MongoDbRepository : IRepository
    {
        private const int TimeoutSec = 60;
        private static IMongoDatabase database;
        private static string databaseName;
        private static MongoClient mongoClient;
        private static IMongoCollection<RetryQueueItemDbo> retryQueueItemsCollection;
        private static IMongoCollection<RetryQueueDbo> retryQueuesCollection;

        public MongoDbRepository(
            string connectionString,
            string dbName,
            string retryQueueCollectionName,
            string retryQueueItemCollectionName)
        {
            databaseName = dbName;
            mongoClient = new MongoClient(connectionString);
            database = mongoClient.GetDatabase(dbName);
            retryQueuesCollection = database.GetCollection<RetryQueueDbo>(retryQueueCollectionName);
            retryQueueItemsCollection = database.GetCollection<RetryQueueItemDbo>(retryQueueItemCollectionName);
        }

        public Type RepositoryType => typeof(MongoDbRepository);

        public async Task CleanDatabaseAsync()
        {
            await mongoClient.DropDatabaseAsync(databaseName).ConfigureAwait(false);
        }

        public async Task<RetryQueueTestModel> GetRetryQueueAsync(RetryDurableTestMessage message)
        {
            var start = DateTime.Now;
            Guid retryQueueId = Guid.Empty;
            RetryQueueDbo retryQueue = new RetryQueueDbo();
            do
            {
                if (DateTime.Now.Subtract(start).Seconds > TimeoutSec)
                {
                    return new RetryQueueTestModel();
                }

                await Task.Delay(100).ConfigureAwait(false);

                var retryQueueCursor = await retryQueuesCollection.FindAsync(x => string.Equals(x.QueueGroupKey, message.Key)).ConfigureAwait(false);
                var retryQueues = await retryQueueCursor.ToListAsync().ConfigureAwait(false);
                if (retryQueues.Any())
                {
                    retryQueue = retryQueues.Single();
                    retryQueueId = retryQueue.Id;
                }
            } while (retryQueueId == Guid.Empty);

            return new RetryQueueTestModel
            {
                Id = retryQueue.Id,
                Status = (RetryQueueStatusTestModel)retryQueue.Status
            };
        }

        public async Task<IList<RetryQueueItemTestModel>> GetRetryQueueItemsAsync(
            Guid retryQueueId,
            Func<IList<RetryQueueItemTestModel>, bool> stopCondition)
        {
            var start = DateTime.Now;
            List<RetryQueueItemTestModel> retryQueueItems = new List<RetryQueueItemTestModel>();
            do
            {
                if (DateTime.Now.Subtract(start).Seconds > TimeoutSec)
                {
                    return null;
                }

                await Task.Delay(100).ConfigureAwait(false);

                var retryQueueItemsCursor = await retryQueueItemsCollection.FindAsync(x => x.RetryQueueId == retryQueueId).ConfigureAwait(false);
                var retryQueueItemsDbo = await retryQueueItemsCursor
                    .ToListAsync()
                    .ConfigureAwait(false);

                retryQueueItems = retryQueueItemsDbo
                    .Select(
                        x =>
                        {
                            return new RetryQueueItemTestModel
                            {
                                AttemptsCount = x.AttemptsCount,
                                Sort = x.Sort,
                                LastExecution = x.LastExecution,
                                Status = (RetryQueueItemStatusTestModel)x.Status
                            };
                        }).ToList();
            } while (stopCondition(retryQueueItems));

            return retryQueueItems;
        }
    }
}