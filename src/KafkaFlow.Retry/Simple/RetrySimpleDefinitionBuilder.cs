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
        => this.Handle(kafkaRetryContext => kafkaRetryContext.Exception is TException);

    public RetrySimpleDefinitionBuilder Handle(Func<RetryContext, bool> func)
    {
        this.retryWhenExceptions.Add(func);
        return this;
    }

    public RetrySimpleDefinitionBuilder Handle<TException>(Func<TException, bool> rule)
        where TException : Exception
        => this.Handle(context => context.Exception is TException ex && rule(ex));

    public RetrySimpleDefinitionBuilder HandleAnyException()
        => this.Handle(kafkaRetryContext => true);

    public RetrySimpleDefinitionBuilder ShouldPauseConsumer(bool pause)
    {
        this.pauseConsumer = pause;
        return this;
    }

    public RetrySimpleDefinitionBuilder TryTimes(int numberOfRetries)
    {
        this.numberOfRetries = numberOfRetries;
        return this;
    }

    public RetrySimpleDefinitionBuilder WithTimeBetweenTriesPlan(Func<int, TimeSpan> timesBetweenTriesPlan)
    {
        this.timeBetweenTriesPlan = timesBetweenTriesPlan;
        return this;
    }

    public RetrySimpleDefinitionBuilder WithTimeBetweenTriesPlan(params TimeSpan[] timeBetweenRetries)
        => this.WithTimeBetweenTriesPlan(
            (retryNumber) =>
                ((retryNumber - 1) < timeBetweenRetries.Length)
                    ? timeBetweenRetries[retryNumber - 1]
                    : timeBetweenRetries[timeBetweenRetries.Length - 1]
        );

    internal RetrySimpleDefinition Build()
    {
        return new RetrySimpleDefinition(
            this.numberOfRetries,
            this.retryWhenExceptions,
            this.pauseConsumer,
            this.timeBetweenTriesPlan
        );
    }
}