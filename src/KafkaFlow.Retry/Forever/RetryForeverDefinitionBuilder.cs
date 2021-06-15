namespace KafkaFlow.Retry
{
    using System;
    using System.Collections.Generic;
    using KafkaFlow.Retry.Forever;

    public class RetryForeverDefinitionBuilder
    {
        private readonly List<Func<RetryContext, bool>> retryWhenExceptions = new List<Func<RetryContext, bool>>();
        private Func<int, TimeSpan> timeBetweenTriesPlan;

        public RetryForeverDefinitionBuilder Handle<TException>()
            where TException : Exception
            => this.Handle(kafkaRetryContext => kafkaRetryContext.Exception is TException);

        public RetryForeverDefinitionBuilder Handle<TException>(Func<TException, bool> rule)
            where TException : Exception
            => this.Handle(context => context.Exception is TException ex && rule(ex));

        public RetryForeverDefinitionBuilder Handle(Func<RetryContext, bool> func)
        {
            this.retryWhenExceptions.Add(func);
            return this;
        }

        public RetryForeverDefinitionBuilder HandleAnyException()
            => this.Handle(kafkaRetryContext => true);

        public RetryForeverDefinitionBuilder WithTimeBetweenTriesPlan(Func<int, TimeSpan> timeBetweenTriesPlan)
        {
            this.timeBetweenTriesPlan = timeBetweenTriesPlan;
            return this;
        }

        public RetryForeverDefinitionBuilder WithTimeBetweenTriesPlan(params TimeSpan[] timeBetweenRetries)
            => this.WithTimeBetweenTriesPlan(
                    (retryNumber) =>
                       ((retryNumber - 1) < timeBetweenRetries.Length)
                           ? timeBetweenRetries[retryNumber - 1]
                           : timeBetweenRetries[timeBetweenRetries.Length - 1]
                );

        internal RetryForeverDefinition Build()
        {
            return new RetryForeverDefinition(
                this.timeBetweenTriesPlan,
                this.retryWhenExceptions
            );
        }
    }
}