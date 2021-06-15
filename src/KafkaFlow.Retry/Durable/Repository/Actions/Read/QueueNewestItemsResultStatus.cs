namespace KafkaFlow.Retry.Durable.Repository.Actions.Read
{
    public enum QueueNewestItemsResultStatus
    {
        None = 0,
        HasNewestItems = 3,
        NoNewestItems = 4,
    }
}