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

        public KafkaRetryDefinitionBuilder ShouldNotPauseConsumer()
        {
            this.pauseConsumer = false;
            return this;
        }

        public KafkaRetryDefinitionBuilder ShouldPauseConsumer()
        {
            this.pauseConsumer = true;
            return this;
        }

        public KafkaRetryDefinitionBuilder TryTimes(int numberOfRetries)
        {
            this.numberOfRetries = numberOfRetries;
            return this;
        }

        public KafkaRetryDefinitionBuilder WasThrown<TException>()
                    where TException : Exception
        {
            this.WasThrown(kafkaRetryContext => kafkaRetryContext.Exception is TException);
            return this;
        }

        public KafkaRetryDefinitionBuilder WasThrown(Func<KafkaRetryContext, bool> func)
        {
            this.retryWhenExceptions.Add(func);
            return this;
        }

        public KafkaRetryDefinitionBuilder WasThrownAnyException()
        {
            this.WasThrown(kafkaRetryContext => true);
            return this;
        }

        public KafkaRetryDefinitionBuilder WithTimeBetweenTriesPlan(Func<int, TimeSpan> timesBetweenTriesPlan)
        {
            this.timeBetweenTriesPlan = timesBetweenTriesPlan;
            return this;
        }

        public KafkaRetryDefinitionBuilder WithTimeBetweenTriesPlan(params TimeSpan[] timeBetweenRetries)
        {
            this.timeBetweenTriesPlan = (retryNumber) =>
               ((retryNumber - 1) < timeBetweenRetries.Length)
                   ? timeBetweenRetries[retryNumber - 1]
                   : timeBetweenRetries[timeBetweenRetries.Length - 1];
            return this;
        }

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