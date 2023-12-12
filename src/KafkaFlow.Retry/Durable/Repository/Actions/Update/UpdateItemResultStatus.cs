namespace KafkaFlow.Retry.Durable.Repository.Actions.Update;

public enum UpdateItemResultStatus
{
    Unknown = 0,
    Updated = 1,
    ItemNotFound = 2,
    QueueNotFound = 3,
    ItemIsNotInWaitingState = 4,
    ItemIsNotTheFirstWaitingInQueue = 5,
    UpdateIsNotAllowed = 6
}