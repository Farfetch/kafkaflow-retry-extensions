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
        {
            this.WasThrown(kafkaRetryContext => kafkaRetryContext.Exception is TException);
            return this;
        }

        public KafkaRetryForeverDefinitionBuilder WasThrown(Func<KafkaRetryContext, bool> func)
        {
            this.retryWhenExceptions.Add(func);
            return this;
        }

        public KafkaRetryForeverDefinitionBuilder WasThrownAnyException()
        {
            this.WasThrown(kafkaRetryContext => true);
            return this;
        }

        public KafkaRetryForeverDefinitionBuilder WithTimeBetweenTriesPlan(Func<int, TimeSpan> timeBetweenTriesPlan)
        {
            this.timeBetweenTriesPlan = timeBetweenTriesPlan;
            return this;
        }

        public KafkaRetryForeverDefinitionBuilder WithTimeBetweenTriesPlan(params TimeSpan[] timeBetweenRetries)
        {
            this.timeBetweenTriesPlan = (retryNumber) =>
                ((retryNumber - 1) < timeBetweenRetries.Length)
                    ? timeBetweenRetries[retryNumber - 1]
                    : timeBetweenRetries[timeBetweenRetries.Length - 1];
            return this;
        }

        internal KafkaRetryDurableDefinition Build()
        {
            return new KafkaRetryDurableDefinition(
                this.timeBetweenTriesPlan,
                this.retryWhenExceptions
            );
        }
    }
}