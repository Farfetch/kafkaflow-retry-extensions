namespace KafkaFlow.Retry.Durable.Repository.Actions.Read;

public enum CheckQueueResultStatus
{
    Unknown = 0,
    DoesNotExist = 1,
    Exists = 2
}