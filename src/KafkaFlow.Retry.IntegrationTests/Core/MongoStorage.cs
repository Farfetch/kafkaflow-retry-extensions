namespace KafkaFlow.Retry.IntegrationTests.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;
    using KafkaFlow.Retry.MongoDb.Model;
    using MongoDB.Driver;
    using Xunit;

    internal static class MongoStorage
    {
        private const int TimeoutSec = 60;
        private static IMongoDatabase database;
        private static string databaseName;
        private static MongoClient mongoClient;
        private static IMongoCollection<RetryQueueItemDbo> retryQueueItemsCollection;
        private static IMongoCollection<RetryQueueDbo> retryQueuesCollection;

        public static void DropDatabase()
        {
            mongoClient.DropDatabase(databaseName);
        }

        public static void Setup(
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

        internal static async Task AssertRetryDurableMessageCreationAsync(RetryDurableTestMessage message, int count)
        {
            RetryQueueDbo retryQueue = await GetRetryQueueAsync(message).ConfigureAwait(false);
            if (retryQueue.Id == Guid.Empty)
            {
                Assert.True(false, "Retry Durable Creation Get Retry Queue cannot be asserted.");
                return;
            }
            var retryQueueItems = await GetRetryQueueItemsAsync(retryQueue.Id, rqi => rqi.Count() != count).ConfigureAwait(false);
            if (retryQueueItems is null)
            {
                Assert.True(false, "Retry Durable Creation Get Retry Queue Item Message cannot be asserted.");
                return;
            }

            Assert.Equal(0, retryQueueItems.Sum(i => i.AttemptsCount));
            Assert.Equal(retryQueueItems.Count() - 1, retryQueueItems.Max(i => i.Sort));
            Assert.True(Enum.Equals(retryQueue.Status, RetryQueueStatus.Active));
            Assert.All(retryQueueItems, i => Enum.Equals(i.Status, RetryQueueItemStatus.Waiting));
        }

        internal static async Task AssertRetryDurableMessageDoneAsync(RetryDurableTestMessage message)
        {
            RetryQueueDbo retryQueue = await GetRetryQueueAsync(message).ConfigureAwait(false);
            if (retryQueue.Id == Guid.Empty)
            {
                Assert.True(false, "Retry Durable Done Get Retry Queue cannot be asserted.");
                return;
            }
            var retryQueueItems = await GetRetryQueueItemsAsync(
                retryQueue.Id,
                rqi =>
                {
                    return rqi.All(x => !Enum.Equals(x.Status, RetryQueueItemStatus.Done));
                }).ConfigureAwait(false);
            if (retryQueueItems is null)
            {
                Assert.True(false, "Retry Durable Done Get Retry Queue Item Message cannot be asserted.");
                return;
            }

            Assert.True(Enum.Equals(retryQueue.Status, RetryQueueStatus.Done));
        }

        internal static async Task AssertRetryDurableMessageRetryingAsync(RetryDurableTestMessage message, int retryCount)
        {
            RetryQueueDbo retryQueue = await GetRetryQueueAsync(message).ConfigureAwait(false);
            if (retryQueue.Id == Guid.Empty)
            {
                Assert.True(false, "Retry Durable Retrying Get Retry Queue cannot be asserted.");
                return;
            }
            var retryQueueItems = await GetRetryQueueItemsAsync(
                retryQueue.Id,
                rqi =>
                {
                    return
                    rqi.Single(x => x.Sort == rqi.Min(i => i.Sort)).LastExecution >
                    rqi.Single(x => x.Sort == rqi.Max(i => i.Sort)).LastExecution;
                }).ConfigureAwait(false);
            if (retryQueueItems is null)
            {
                Assert.True(false, "Retry Durable Retrying Get Retry Queue Item Message cannot be asserted.");
                return;
            }

            Assert.Equal(retryCount, retryQueueItems.Where(x => x.Sort == 0).Sum(i => i.AttemptsCount));
            Assert.Equal(0, retryQueueItems.Where(x => x.Sort != 0).Sum(i => i.AttemptsCount));
            Assert.Equal(retryQueueItems.Count() - 1, retryQueueItems.Max(i => i.Sort));
            Assert.True(Enum.Equals(retryQueue.Status, RetryQueueStatus.Active));
            Assert.All(retryQueueItems, i => Enum.Equals(i.Status, RetryQueueItemStatus.Waiting));
        }

        private static async Task<RetryQueueDbo> GetRetryQueueAsync(RetryDurableTestMessage message)
        {
            var start = DateTime.Now;
            Guid retryQueueId = Guid.Empty;
            RetryQueueDbo retryQueue = new RetryQueueDbo();
            do
            {
                if (DateTime.Now.Subtract(start).Seconds > TimeoutSec)
                {
                    Assert.True(false, "Message is not in repository RetryQueue.");
                    return retryQueue;
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

            return retryQueue;
        }

        private static async Task<List<RetryQueueItemDbo>> GetRetryQueueItemsAsync(
            Guid retryQueueId,
            Func<List<RetryQueueItemDbo>, bool> stopCondition)
        {
            var start = DateTime.Now;
            List<RetryQueueItemDbo> retryQueueItems = new List<RetryQueueItemDbo>();
            do
            {
                if (DateTime.Now.Subtract(start).Seconds > TimeoutSec)
                {
                    Assert.True(false, "Message is not in repository RetryQueueItems.");
                    return null;
                }

                await Task.Delay(100).ConfigureAwait(false);

                var retryQueueItemsCursor = await retryQueueItemsCollection.FindAsync(x => x.RetryQueueId == retryQueueId).ConfigureAwait(false);
                retryQueueItems = await retryQueueItemsCursor.ToListAsync().ConfigureAwait(false);
            } while (stopCondition(retryQueueItems));

            return retryQueueItems;
        }
    }
}