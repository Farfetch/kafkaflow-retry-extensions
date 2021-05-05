namespace KafkaFlow.Retry.Durable.Repository.Actions.Read
{
    public enum QueuePendingItemsResultStatus
    {
        None = 0,
        NoPendingItems = 1,
        HasPendingItems = 2,
    }
}
