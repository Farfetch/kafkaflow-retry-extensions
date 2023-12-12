namespace KafkaFlow.Retry.Durable.Repository.Actions.Read;

public enum GetQueuesSortOption
{
    None = 0,
    ByLastExecution_Ascending = 1,
    ByCreationDate_Descending = 2,
}