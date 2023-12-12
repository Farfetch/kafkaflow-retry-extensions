using System;
using KafkaFlow.Retry.Durable.Definitions;

namespace KafkaFlow.Retry;

public class RetryDurableRetryPlanBeforeDefinitionBuilder
{
    private int numberOfRetries;
    private bool pauseConsumer;
    private Func<int, TimeSpan> timeBetweenTriesPlan;

    public RetryDurableRetryPlanBeforeDefinitionBuilder ShouldPauseConsumer(bool pause)
    {
            pauseConsumer = pause;
            return this;
        }

    public RetryDurableRetryPlanBeforeDefinitionBuilder TryTimes(int numberOfRetries)
    {
            this.numberOfRetries = numberOfRetries;
            return this;
        }

    public RetryDurableRetryPlanBeforeDefinitionBuilder WithTimeBetweenTriesPlan(Func<int, TimeSpan> timeBetweenTriesPlan)
    {
            this.timeBetweenTriesPlan = timeBetweenTriesPlan;
            return this;
        }

    public RetryDurableRetryPlanBeforeDefinitionBuilder WithTimeBetweenTriesPlan(params TimeSpan[] timeBetweenRetries)
        => WithTimeBetweenTriesPlan(
            (retryNumber) =>
                ((retryNumber - 1) < timeBetweenRetries.Length)
                    ? timeBetweenRetries[retryNumber - 1]
                    : timeBetweenRetries[timeBetweenRetries.Length - 1]
        );

    internal RetryDurableRetryPlanBeforeDefinition Build()
    {
            return new RetryDurableRetryPlanBeforeDefinition(
                timeBetweenTriesPlan,
                numberOfRetries,
                pauseConsumer
            );
        }
}