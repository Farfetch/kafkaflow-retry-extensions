namespace KafkaFlow.Retry.Durable.Polling
{
    internal static class PollingConstants
    {
        public const string KafkaRetryDurableQueueRepository = "KafkaRetryDurableQueueRepository";
        public const string NonBlockRetryPolicyConfigKey = "NonBlockRetryPolicyConfig";
        public const string PollingConfigKey = "PollingConfig";
        public const string RetryDurableConsumer = "RetryDurableConsumer";
        public const string RetryPolicyBuilderKey = "RetryPolicyBuilder";
        public const string RetryProducerKey = "QueueProducer";
    }
}