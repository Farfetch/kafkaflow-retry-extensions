using System;
using Dawn;
using KafkaFlow.Retry.Durable.Repository.Actions.Create;

namespace KafkaFlow.Retry.SqlServer.Model.Factories;

internal sealed class RetryQueueItemDboFactory : IRetryQueueItemDboFactory
{
    public RetryQueueItemDbo Create(SaveToQueueInput input, long retryQueueId, Guid retryQueueDomainId)
    {
        Guard.Argument(input, nameof(input)).NotNull();
        Guard.Argument(retryQueueId, nameof(retryQueueId)).Positive();
        Guard.Argument(retryQueueDomainId, nameof(retryQueueDomainId)).NotDefault();

        return new RetryQueueItemDbo
        {
            IdDomain = Guid.NewGuid(),
            CreationDate = input.CreationDate,
            LastExecution = input.LastExecution,
            ModifiedStatusDate = input.ModifiedStatusDate,
            AttemptsCount = input.AttemptsCount,
            RetryQueueId = retryQueueId,
            DomainRetryQueueId = retryQueueDomainId,
            Status = input.ItemStatus,
            SeverityLevel = input.SeverityLevel,
            Description = input.Description
        };
    }
}