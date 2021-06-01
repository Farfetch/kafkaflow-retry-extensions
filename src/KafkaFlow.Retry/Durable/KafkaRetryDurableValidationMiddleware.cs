namespace KafkaFlow.Retry.Durable
{
    using System;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.Durable.Common;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;
    using KafkaFlow.Retry.Durable.Repository.Model;

    internal class KafkaRetryDurableValidationMiddleware : IMessageMiddleware
    {
        private readonly ILogHandler logHandler;
        private readonly IKafkaRetryDurableQueueRepository retryDurableQueueRepository;

        public KafkaRetryDurableValidationMiddleware(
            ILogHandler logHandler,
            IKafkaRetryDurableQueueRepository retryDurableQueueRepository)
        {
            this.logHandler = logHandler;
            this.retryDurableQueueRepository = retryDurableQueueRepository;
        }

        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            var queueId = Guid.Parse(context.Headers[KafkaRetryDurableConstants.QueueId].ByteArrayToString());
            var itemId = Guid.Parse(context.Headers[KafkaRetryDurableConstants.ItemId].ByteArrayToString());
            var attemptsCount = int.Parse(context.Headers[KafkaRetryDurableConstants.AttemptsCount].ByteArrayToString());
            var sort = int.Parse(context.Headers[KafkaRetryDurableConstants.Sort].ByteArrayToString());

            try
            {
                var pendingItems = await this
                    .ThereArePendingItemsAsync(
                        queueId,
                        itemId,
                        sort)
                    .ConfigureAwait(false);

                if (pendingItems)
                {
                    await this
                        .UpdateAsync(
                            RetryQueueItemStatus.Waiting,
                            queueId,
                            itemId,
                            attemptsCount
                        ).ConfigureAwait(false);

                    return;
                }

                await next(context).ConfigureAwait(false);

                await this
                    .UpdateAsync(
                        RetryQueueItemStatus.Done,
                        queueId,
                        itemId,
                        ++attemptsCount)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await this
                    .UpdateAsync(
                        RetryQueueItemStatus.Waiting,
                        queueId,
                        itemId,
                        ++attemptsCount,
                        exception)
                    .ConfigureAwait(false);
            }
        }

        private async Task<bool> ThereArePendingItemsAsync(
            Guid queueId,
            Guid itemId,
            int sort)
        {
            var queuePendingItemsInput = new
                       QueuePendingItemsInput(
                           queueId,
                           itemId,
                           sort
                       );

            var queuePendingItemsResult = await this
                .retryDurableQueueRepository
                .CheckQueuePendingItemsAsync(queuePendingItemsInput)
                .ConfigureAwait(false);

            return queuePendingItemsResult.Status == QueuePendingItemsResultStatus.HasPendingItems;
        }

        private async Task UpdateAsync(
            RetryQueueItemStatus targetStatus,
            Guid queueId,
            Guid itemId,
            int attemptsCount,
            Exception exception = null)
        {
            await this
                .retryDurableQueueRepository
                .UpdateItemAsync(
                    new UpdateItemExecutionInfoInput(
                        queueId,
                        itemId,
                        targetStatus,
                        attemptsCount,
                        DateTime.UtcNow,
                        exception?.ToString()
                    )
                )
                .ConfigureAwait(false);
        }
    }
}