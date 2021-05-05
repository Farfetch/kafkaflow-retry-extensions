namespace KafkaFlow.Retry.Durable.Repository.Actions.Update
{
    public enum UpdateQueueResultStatus
    {
        Unknown = 0,
        Updated = 1,
        QueueNotFound = 2,
        QueueIsNotActive = 3,
        QueueHasNoItemsWaiting = 4,
        NotUpdated = 5,
        UpdateIsNotAllowed = 6,
        FailedToUpdateAllItems = 7,
        FailedToUpdateItems = 8,
        AllItemsUpdatedButFailedToUpdateQueue = 9
    }
}
