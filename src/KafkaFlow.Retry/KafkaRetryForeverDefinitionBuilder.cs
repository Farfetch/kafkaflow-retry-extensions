namespace KafkaFlow.Retry
{
    using System;
    using System.Collections.Generic;

    using KafkaFlow.Retry.Forever;

    public class KafkaRetryForeverDefinitionBuilder
    {
        private readonly List<Func<KafkaRetryContext, bool>> retryWhenExceptions = new List<Func<KafkaRetryContext, bool>>();
        private Func<int, TimeSpan> timeBetweenTriesPlan;

        public KafkaRetryForeverDefinitionBuilder Handle<TException>()
            where TException : Exception
            => this.Handle(kafkaRetryContext => kafkaRetryContext.Exception is TException);

        public KafkaRetryForeverDefinitionBuilder Handle<TException>(Func<TException, bool> rule)
            where TException : Exception
            => this.Handle(context => context.Exception is TException ex && rule(ex));

        public KafkaRetryForeverDefinitionBuilder Handle(Func<KafkaRetryContext, bool> func)
        {
            this.retryWhenExceptions.Add(func);
            return this;
        }

        public KafkaRetryForeverDefinitionBuilder HandleAnyException()
            => this.Handle(kafkaRetryContext => true);

        public KafkaRetryForeverDefinitionBuilder WithTimeBetweenTriesPlan(Func<int, TimeSpan> timeBetweenTriesPlan)
        {
            this.timeBetweenTriesPlan = timeBetweenTriesPlan;
            return this;
        }

        public KafkaRetryForeverDefinitionBuilder WithTimeBetweenTriesPlan(params TimeSpan[] timeBetweenRetries)
            => this.WithTimeBetweenTriesPlan(
                    (retryNumber) =>
                       ((retryNumber - 1) < timeBetweenRetries.Length)
                           ? timeBetweenRetries[retryNumber - 1]
                           : timeBetweenRetries[timeBetweenRetries.Length - 1]
                );

        internal KafkaRetryForeverDefinition Build()
        {
            return new KafkaRetryForeverDefinition(
                this.timeBetweenTriesPlan,
                this.retryWhenExceptions
            );
        }
    }
}