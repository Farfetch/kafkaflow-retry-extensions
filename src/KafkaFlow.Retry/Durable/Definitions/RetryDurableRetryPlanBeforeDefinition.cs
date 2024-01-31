using System;
using Dawn;

namespace KafkaFlow.Retry.Durable.Definitions;

internal class RetryDurableRetryPlanBeforeDefinition
{
    public RetryDurableRetryPlanBeforeDefinition(
        Func<int, TimeSpan> timeBetweenTriesPlan,
        int numberOfRetries,
        bool pauseConsumer
    )
    {
        Guard.Argument(numberOfRetries).NotZero()
            .NotNegative(value => "The number of retries should be higher than zero");
        Guard.Argument(timeBetweenTriesPlan).NotNull("A plan of times betwwen tries should be defined");

        TimeBetweenTriesPlan = timeBetweenTriesPlan;
        NumberOfRetries = numberOfRetries;
        PauseConsumer = pauseConsumer;
    }

    public int NumberOfRetries { get; }

    public bool PauseConsumer { get; }

    public Func<int, TimeSpan> TimeBetweenTriesPlan { get; }
}