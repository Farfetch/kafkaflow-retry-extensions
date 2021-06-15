namespace KafkaFlow.Retry.Durable
{
    using System;
    using System.Threading.Tasks;
    using Dawn;
    using KafkaFlow.Retry.Durable.Encoders;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;
    using KafkaFlow.Retry.Durable.Repository.Model;

    internal class RetryDurableConsumerGuaranteeOrderedMiddleware : IMessageMiddleware
    {
        private readonly ILogHandler logHandler;
        private readonly IRetryDurableQueueRepository retryDurableQueueRepository;
        private readonly IUtf8Encoder utf8Encoder;

        public RetryDurableConsumerGuaranteeOrderedMiddleware(
            ILogHandler logHandler,
            IRetryDurableQueueRepository retryDurableQueueRepository,
            IUtf8Encoder utf8Encoder)
        {
            Guard.Argument(logHandler).NotNull();
            Guard.Argument(retryDurableQueueRepository).NotNull();
            Guard.Argument(utf8Encoder).NotNull();

            this.logHandler = logHandler;
            this.retryDurableQueueRepository = retryDurableQueueRepository;
            this.utf8Encoder = utf8Encoder;
        }

        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            var queueId = Guid.Parse(this.utf8Encoder.Decode(context.Headers[RetryDurableConstants.QueueId]));
            var itemId = Guid.Parse(this.utf8Encoder.Decode(context.Headers[RetryDurableConstants.ItemId]));
            var attemptsCount = int.Parse(this.utf8Encoder.Decode(context.Headers[RetryDurableConstants.AttemptsCount]));
            var sort = int.Parse(this.utf8Encoder.Decode(context.Headers[RetryDurableConstants.Sort]));

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