using System.Collections.Generic;
using System.Linq;
using Dawn;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.Postgres.Model.Factories;

internal sealed class RetryQueueItemMessageHeaderDboFactory : IRetryQueueItemMessageHeaderDboFactory
{
    public IEnumerable<RetryQueueItemMessageHeaderDbo> Create(IEnumerable<MessageHeader> headers, long retryQueueItemId)
    {
            Guard.Argument(headers).NotNull();
            Guard.Argument(retryQueueItemId, nameof(retryQueueItemId)).Positive();

            return headers.Select(h => this.Adapt(h, retryQueueItemId));
        }

    private RetryQueueItemMessageHeaderDbo Adapt(MessageHeader header, long retryQueueItemId)
    {
            Guard.Argument(header).NotNull();

            return new RetryQueueItemMessageHeaderDbo
            {
                Key = header.Key,
                Value = header.Value,
                RetryQueueItemMessageId = retryQueueItemId
            };
        }
}