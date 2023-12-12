namespace KafkaFlow.Retry.Durable.Repository.Actions.Create;

public enum AddIfQueueExistsResultStatus
{
    Unknown = 0,
    Added = 1,
    NoPendingMembers = 2
}