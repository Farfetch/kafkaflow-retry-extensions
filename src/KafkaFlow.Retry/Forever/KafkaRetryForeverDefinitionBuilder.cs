namespace KafkaFlow.Retry.Forever
{
    using System;
    using System.Collections.Generic;

    public class KafkaRetryForeverDefinitionBuilder
    {
        private readonly List<Func<KafkaRetryContext, bool>> retryWhenExceptions = new List<Func<KafkaRetryContext, bool>>();
        private TimeSpan[] timesBetweenRetries;

        public KafkaRetryForeverDefinitionBuilder WasThrown<TException>()
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

        public KafkaRetryForeverDefinitionBuilder WithTimesBetweenTries(params TimeSpan[] timesBetweenRetries)
        {
            this.timesBetweenRetries = timesBetweenRetries;
            return this;
        }

        internal KafkaRetryForeverDefinition Build()
        {
            return new KafkaRetryForeverDefinition(
                this.timesBetweenRetries,
                this.retryWhenExceptions
            );
        }
    }
}