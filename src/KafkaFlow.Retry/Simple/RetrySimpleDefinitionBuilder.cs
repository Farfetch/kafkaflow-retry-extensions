using System;
using System.Collections.Generic;
using KafkaFlow.Retry.Simple;

namespace KafkaFlow.Retry;

public class RetrySimpleDefinitionBuilder
{
    private readonly List<Func<RetryContext, bool>> _retryWhenExceptions = new List<Func<RetryContext, bool>>();
    private int _numberOfRetries;
    private bool _pauseConsumer;
    private Func<int, TimeSpan> _timeBetweenTriesPlan;

    public RetrySimpleDefinitionBuilder Handle<TException>()
        where TException : Exception
        => Handle(kafkaRetryContext => kafkaRetryContext.Exception is TException);

    public RetrySimpleDefinitionBuilder Handle(Func<RetryContext, bool> func)
    {
        _retryWhenExceptions.Add(func);
        return this;
    }

    public RetrySimpleDefinitionBuilder Handle<TException>(Func<TException, bool> rule)
        where TException : Exception
        => Handle(context => context.Exception is TException ex && rule(ex));

    public RetrySimpleDefinitionBuilder HandleAnyException()
        => Handle(kafkaRetryContext => true);

    public RetrySimpleDefinitionBuilder ShouldPauseConsumer(bool pause)
    {
        _pauseConsumer = pause;
        return this;
    }

    public RetrySimpleDefinitionBuilder TryTimes(int numberOfRetries)
    {
        _numberOfRetries = numberOfRetries;
        return this;
    }

    public RetrySimpleDefinitionBuilder WithTimeBetweenTriesPlan(Func<int, TimeSpan> timesBetweenTriesPlan)
    {
        _timeBetweenTriesPlan = timesBetweenTriesPlan;
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
            _numberOfRetries,
            _retryWhenExceptions,
            _pauseConsumer,
            _timeBetweenTriesPlan
        );
    }
}