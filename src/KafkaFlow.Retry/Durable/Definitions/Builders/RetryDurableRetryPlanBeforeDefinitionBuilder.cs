using System;
using KafkaFlow.Retry.Durable.Definitions;

namespace KafkaFlow.Retry;

public class RetryDurableRetryPlanBeforeDefinitionBuilder
{
    private int _numberOfRetries;
    private bool _pauseConsumer;
    private Func<int, TimeSpan> _timeBetweenTriesPlan;

    public RetryDurableRetryPlanBeforeDefinitionBuilder ShouldPauseConsumer(bool pause)
    {
            _pauseConsumer = pause;
            return this;
        }

    public RetryDurableRetryPlanBeforeDefinitionBuilder TryTimes(int numberOfRetries)
    {
            _numberOfRetries = numberOfRetries;
            return this;
        }

    public RetryDurableRetryPlanBeforeDefinitionBuilder WithTimeBetweenTriesPlan(Func<int, TimeSpan> timeBetweenTriesPlan)
    {
            _timeBetweenTriesPlan = timeBetweenTriesPlan;
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
                _timeBetweenTriesPlan,
                _numberOfRetries,
                _pauseConsumer
            );
        }
}