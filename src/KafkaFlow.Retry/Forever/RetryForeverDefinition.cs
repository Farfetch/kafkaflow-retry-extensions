using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;

namespace KafkaFlow.Retry.Forever;

internal class RetryForeverDefinition
{
    private readonly IReadOnlyCollection<Func<RetryContext, bool>> retryWhenExceptions;

    public RetryForeverDefinition(
        Func<int, TimeSpan> timeBetweenTriesPlan,
        IReadOnlyCollection<Func<RetryContext, bool>> retryWhenExceptions
    )
    {
            Guard.Argument(retryWhenExceptions).NotNull("At least an exception should be defined");
            Guard.Argument(retryWhenExceptions.Count).NotNegative(value => "At least an exception should be defined");
            Guard.Argument(timeBetweenTriesPlan).NotNull("A plan of times betwwen tries should be defined");

            TimeBetweenTriesPlan = timeBetweenTriesPlan;
            this.retryWhenExceptions = retryWhenExceptions;
        }

    public Func<int, TimeSpan> TimeBetweenTriesPlan { get; }

    public bool ShouldRetry(RetryContext kafkaRetryContext) =>
        retryWhenExceptions.Any(rule => rule(kafkaRetryContext));
}