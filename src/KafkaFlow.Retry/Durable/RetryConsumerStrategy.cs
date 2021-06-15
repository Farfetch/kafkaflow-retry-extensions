namespace KafkaFlow.Retry
{
    public enum RetryConsumerStrategy
    {
        /// <summary>
        /// The consumed order of messages for the same queue is preserved when the retry runs
        /// </summary>
        GuaranteeOrderedConsumption = 1,

        /// <summary>
        /// The last consumed message has prevalence over previous consumed messages
        /// </summary>
        LatestConsumption = 2
    }
}