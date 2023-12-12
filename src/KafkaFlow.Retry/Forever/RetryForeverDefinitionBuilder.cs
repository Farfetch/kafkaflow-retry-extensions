using System;
using System.Collections.Generic;
using KafkaFlow.Retry.Forever;

namespace KafkaFlow.Retry;

public class RetryForeverDefinitionBuilder
{
    private readonly List<Func<RetryContext, bool>> _retryWhenExceptions = new List<Func<RetryContext, bool>>();
    private Func<int, TimeSpan> _timeBetweenTriesPlan;

    public RetryForeverDefinitionBuilder Handle<TException>()
        where TException : Exception
        => Handle(kafkaRetryContext => kafkaRetryContext.Exception is TException);

    public RetryForeverDefinitionBuilder Handle<TException>(Func<TException, bool> rule)
        where TException : Exception
        => Handle(context => context.Exception is TException ex && rule(ex));

    public RetryForeverDefinitionBuilder Handle(Func<RetryContext, bool> func)
    {
            _retryWhenExceptions.Add(func);
            return this;
        }

    public RetryForeverDefinitionBuilder HandleAnyException()
        => Handle(kafkaRetryContext => true);

    public RetryForeverDefinitionBuilder WithTimeBetweenTriesPlan(Func<int, TimeSpan> timeBetweenTriesPlan)
    {
            _timeBetweenTriesPlan = timeBetweenTriesPlan;
            return this;
        }

    public RetryForeverDefinitionBuilder WithTimeBetweenTriesPlan(params TimeSpan[] timeBetweenRetries)
        => WithTimeBetweenTriesPlan(
            (retryNumber) =>
                ((retryNumber - 1) < timeBetweenRetries.Length)
                    ? timeBetweenRetries[retryNumber - 1]
                    : timeBetweenRetries[timeBetweenRetries.Length - 1]
        );

    internal RetryForeverDefinition Build()
    {
            return new RetryForeverDefinition(
                _timeBetweenTriesPlan,
                _retryWhenExceptions
            );
        }
}