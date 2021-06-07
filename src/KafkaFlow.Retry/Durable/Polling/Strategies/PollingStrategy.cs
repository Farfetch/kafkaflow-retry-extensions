namespace KafkaFlow.Retry
{
    public enum PollingStrategy
    {
        /// <summary>
        /// The consumed order of messages for the same queue is preserved when the retry runs
        /// </summary>
        KeepConsumptionOrder = 1,

        /// <summary>
        /// The last consumed message has prevalence over previous consumed messages
        /// </summary>
        LastConsumed = 2
    }
}