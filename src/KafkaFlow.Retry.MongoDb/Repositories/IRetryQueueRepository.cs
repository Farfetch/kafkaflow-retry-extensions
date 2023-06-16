﻿namespace KafkaFlow.Retry.MongoDb.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.MongoDb.Model;
    using MongoDB.Driver;

    internal interface IRetryQueueRepository
    {
        Task<RetryQueueDbo> GetQueueAsync(string queueGroupKey);

        Task<IEnumerable<RetryQueueDbo>> GetTopSortedQueuesAsync(RetryQueueStatus status, GetQueuesSortOption sortOption, string searchGroupKey, int top);

        Task<UpdateResult> UpdateLastExecutionAsync(Guid queueId, DateTime lastExecution);

        Task<UpdateResult> UpdateStatusAndLastExecutionAsync(Guid queueId, RetryQueueStatus status, DateTime lastExecution);

        Task<UpdateResult> UpdateStatusAsync(Guid queueId, RetryQueueStatus status);
    }
}