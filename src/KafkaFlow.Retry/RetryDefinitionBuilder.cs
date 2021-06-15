namespace KafkaFlow.Retry
{
    using System;
    using System.Collections.Generic;

    public class RetryDefinitionBuilder
    {
        private readonly List<Func<RetryContext, bool>> retryWhenExceptions = new List<Func<RetryContext, bool>>();
        private int numberOfRetries;
        private bool pauseConsumer;
        private Func<int, TimeSpan> timeBetweenTriesPlan;

        public RetryDefinitionBuilder Handle<TException>()
            where TException : Exception
            => this.Handle(kafkaRetryContext => kafkaRetryContext.Exception is TException);

        public RetryDefinitionBuilder Handle(Func<RetryContext, bool> func)
        {
            this.retryWhenExceptions.Add(func);
            return this;
        }

        public RetryDefinitionBuilder Handle<TException>(Func<TException, bool> rule)
            where TException : Exception
            => this.Handle(context => context.Exception is TException ex && rule(ex));

        public RetryDefinitionBuilder HandleAnyException()
            => this.Handle(kafkaRetryContext => true);

        public RetryDefinitionBuilder ShouldPauseConsumer(bool pause)
        {
            this.pauseConsumer = pause;
            return this;
        }

        public RetryDefinitionBuilder TryTimes(int numberOfRetries)
        {
            this.numberOfRetries = numberOfRetries;
            return this;
        }

        public RetryDefinitionBuilder WithTimeBetweenTriesPlan(Func<int, TimeSpan> timesBetweenTriesPlan)
        {
            this.timeBetweenTriesPlan = timesBetweenTriesPlan;
            return this;
        }

        public RetryDefinitionBuilder WithTimeBetweenTriesPlan(params TimeSpan[] timeBetweenRetries)
            => this.WithTimeBetweenTriesPlan(
                    (retryNumber) =>
                       ((retryNumber - 1) < timeBetweenRetries.Length)
                           ? timeBetweenRetries[retryNumber - 1]
                           : timeBetweenRetries[timeBetweenRetries.Length - 1]
                );

        internal RetryDefinition Build()
        {
            return new RetryDefinition(
                this.numberOfRetries,
                this.retryWhenExceptions,
                this.pauseConsumer,
                this.timeBetweenTriesPlan
            );
        }
    }
}