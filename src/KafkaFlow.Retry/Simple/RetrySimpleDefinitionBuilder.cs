using System;
using System.Collections.Generic;
using KafkaFlow.Retry.Simple;

namespace KafkaFlow.Retry;

public class RetrySimpleDefinitionBuilder
{
    private readonly List<Func<RetryContext, bool>> retryWhenExceptions = new List<Func<RetryContext, bool>>();
    private int numberOfRetries;
    private bool pauseConsumer;
    private Func<int, TimeSpan> timeBetweenTriesPlan;

    public RetrySimpleDefinitionBuilder Handle<TException>()
        where TException : Exception
        => Handle(kafkaRetryContext => kafkaRetryContext.Exception is TException);

    public RetrySimpleDefinitionBuilder Handle(Func<RetryContext, bool> func)
    {
        retryWhenExceptions.Add(func);
        return this;
    }

    public RetrySimpleDefinitionBuilder Handle<TException>(Func<TException, bool> rule)
        where TException : Exception
        => Handle(context => context.Exception is TException ex && rule(ex));

    public RetrySimpleDefinitionBuilder HandleAnyException()
        => Handle(kafkaRetryContext => true);

    public RetrySimpleDefinitionBuilder ShouldPauseConsumer(bool pause)
    {
        pauseConsumer = pause;
        return this;
    }

    public RetrySimpleDefinitionBuilder TryTimes(int numberOfRetries)
    {
        this.numberOfRetries = numberOfRetries;
        return this;
    }

    public RetrySimpleDefinitionBuilder WithTimeBetweenTriesPlan(Func<int, TimeSpan> timesBetweenTriesPlan)
    {
        timeBetweenTriesPlan = timesBetweenTriesPlan;
        return this;
    }

    public RetrySimpleDefinitionBuilder WithTimeBetweenTriesPlan(params TimeSpan[] timeBetweenRetries)
        => WithTimeBetweenTriesPlan(
            (retryNumber) =>
                ((retryNumber - 1) < timeBetweenRetries.Length)
                    ? timeBetweenRetries[retryNumber - 1]
                    : timeBetweenRetries[timeBetweenRetries.Length - 1]
        );

    internal RetrySimpleDefinition Build()
    {
        return new RetrySimpleDefinition(
            numberOfRetries,
            retryWhenExceptions,
            pauseConsumer,
            timeBetweenTriesPlan
        );
    }
}