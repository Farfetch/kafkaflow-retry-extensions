namespace KafkaFlow.Retry.Durable
{
    internal static class KafkaRetryDurableConstants
    {
        public const string AttemptsCount = "Kafka-Flow-Retry-Durable-Attempts-Count";
        public const string EmbeddedConsumerGroupId = "kafka-flow-retry-durable-consumer";
        public const string EmbeddedConsumerName = "kafka-flow-retry-durable-consumer";
        public const string EmbeddedProducerName = "kafka-flow-retry-durable-producer";
        public const string ItemId = "Kafka-Flow-Retry-Durable-Item-Id";
        public const string MessageType = "Kafka-Flow-Retry-Durable-Original-Message-Type";
        public const string QueueId = "Kafka-Flow-Retry-Durable-Queue-Id";
    }
}