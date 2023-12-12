namespace KafkaFlow.Retry.Durable.Repository.Actions.Read;

public enum GetQueuesSortOption
{
    None = 0,
    ByLastExecutionAscending = 1,
    ByCreationDateDescending = 2
}