namespace KafkaFlow.Retry
{
    using System;
    using System.Collections.Generic;

    public class KafkaRetryDefinitionBuilder
    {
        private readonly List<Func<KafkaRetryContext, bool>> retryWhenExceptions = new List<Func<KafkaRetryContext, bool>>();
        private int numberOfRetries;
        private bool pauseConsumer;
        private Func<int, TimeSpan> timeBetweenTriesPlan;

        public KafkaRetryDefinitionBuilder Handle<TException>()
            where TException : Exception
            => this.Handle(kafkaRetryContext => kafkaRetryContext.Exception is TException);

        public KafkaRetryDefinitionBuilder Handle(Func<KafkaRetryContext, bool> func)
        {
            this.retryWhenExceptions.Add(func);
            return this;
        }

        public KafkaRetryDefinitionBuilder Handle<TException>(Func<TException, bool> rule)
            where TException : Exception
            => this.Handle(context => context.Exception is TException ex && rule(ex));

        public KafkaRetryDefinitionBuilder HandleAnyException()
            => this.Handle(kafkaRetryContext => true);

        public KafkaRetryDefinitionBuilder ShouldPauseConsumer(bool pause)
        {
            this.pauseConsumer = pause;
            return this;
        }

        public KafkaRetryDefinitionBuilder TryTimes(int numberOfRetries)
        {
            this.numberOfRetries = numberOfRetries;
            return this;
        }

        public KafkaRetryDefinitionBuilder WithTimeBetweenTriesPlan(Func<int, TimeSpan> timesBetweenTriesPlan)
        {
            this.timeBetweenTriesPlan = timesBetweenTriesPlan;
            return this;
        }

        public KafkaRetryDefinitionBuilder WithTimeBetweenTriesPlan(params TimeSpan[] timeBetweenRetries)
            => this.WithTimeBetweenTriesPlan(
                    (retryNumber) =>
                       ((retryNumber - 1) < timeBetweenRetries.Length)
                           ? timeBetweenRetries[retryNumber - 1]
                           : timeBetweenRetries[timeBetweenRetries.Length - 1]
                );

        internal KafkaRetryDefinition Build()
        {
            return new KafkaRetryDefinition(
                this.numberOfRetries,
                this.retryWhenExceptions,
                this.pauseConsumer,
                this.timeBetweenTriesPlan
            );
        }
    }
}