using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using KafkaFlow.Retry.Durable.Repository;

namespace KafkaFlow.Retry.Durable.Definitions;

internal class RetryDurableDefinition
{
    private readonly IReadOnlyCollection<Func<RetryContext, bool>> _retryWhenExceptions;

    public RetryDurableDefinition(
        IReadOnlyCollection<Func<RetryContext, bool>> retryWhenExceptions,
        RetryDurableRetryPlanBeforeDefinition retryDurableRetryPlanBeforeDefinition,
        IRetryDurableQueueRepository retryDurableQueueRepository)
    {
            Guard.Argument(retryWhenExceptions).NotNull("At least an exception should be defined");
            Guard.Argument(retryWhenExceptions.Count).NotNegative(value => "At least an exception should be defined");
            Guard.Argument(retryDurableRetryPlanBeforeDefinition).NotNull();
            Guard.Argument(retryDurableQueueRepository).NotNull();

            _retryWhenExceptions = retryWhenExceptions;
            RetryDurableRetryPlanBeforeDefinition = retryDurableRetryPlanBeforeDefinition;
            RetryDurableQueueRepository = retryDurableQueueRepository;
        }

    public IRetryDurableQueueRepository RetryDurableQueueRepository { get; }

    public RetryDurableRetryPlanBeforeDefinition RetryDurableRetryPlanBeforeDefinition { get; }

    public bool ShouldRetry(RetryContext kafkaRetryContext) =>
        _retryWhenExceptions.Any(rule => rule(kafkaRetryContext));
}