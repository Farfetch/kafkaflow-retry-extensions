namespace KafkaFlow.Retry.Durable
{
    using System;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.Durable.Common;
    using KafkaFlow.Retry.Durable.Repository;
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
            try
            {
                await next(context).ConfigureAwait(false);

                await UpdateAsync(context, RetryQueueItemStatus.Done).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await UpdateAsync(context, RetryQueueItemStatus.Waiting, exception).ConfigureAwait(false);
            }
        }

        private async Task UpdateAsync(
            IMessageContext context,
            RetryQueueItemStatus targetStatus,
            Exception exception = null)
        {
            var queueId = Guid.Parse(context.Headers[KafkaRetryDurableConstants.QueueId].ByteArrayToString());
            var itemId = Guid.Parse(context.Headers[KafkaRetryDurableConstants.ItemId].ByteArrayToString());
            var attemptsCount = int.Parse(context.Headers[KafkaRetryDurableConstants.AttemptsCount].ByteArrayToString());

            await this
                .retryDurableQueueRepository
                .UpdateItemAsync(
                    new UpdateItemExecutionInfoInput(
                        queueId,
                        itemId,
                        targetStatus,
                        ++attemptsCount,
                        DateTime.UtcNow,
                        exception?.ToString()
                    )
                )
                .ConfigureAwait(false);
        }
    }
}