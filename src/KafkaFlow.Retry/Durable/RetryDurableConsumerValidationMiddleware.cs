using System;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.Durable.Encoders;
using KafkaFlow.Retry.Durable.Repository;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.Durable;

internal class RetryDurableConsumerValidationMiddleware : IMessageMiddleware
{
    private readonly ILogHandler logHandler;
    private readonly IRetryDurableQueueRepository retryDurableQueueRepository;
    private readonly IUtf8Encoder utf8Encoder;

    public RetryDurableConsumerValidationMiddleware(
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
            var queueId = Guid.Parse(utf8Encoder.Decode(context.Headers[RetryDurableConstants.QueueId]));
            var itemId = Guid.Parse(utf8Encoder.Decode(context.Headers[RetryDurableConstants.ItemId]));
            var attemptsCount = int.Parse(utf8Encoder.Decode(context.Headers[RetryDurableConstants.AttemptsCount]));

            try
            {
                await next(context).ConfigureAwait(false);

                await UpdateAsync(
                        RetryQueueItemStatus.Done,
                        queueId,
                        itemId,
                        ++attemptsCount)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await UpdateAsync(
                        RetryQueueItemStatus.Waiting,
                        queueId,
                        itemId,
                        ++attemptsCount,
                        exception)
                    .ConfigureAwait(false);
            }
        }

    private async Task UpdateAsync(
        RetryQueueItemStatus targetStatus,
        Guid queueId,
        Guid itemId,
        int attemptsCount,
        Exception exception = null)
    {
            await retryDurableQueueRepository
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