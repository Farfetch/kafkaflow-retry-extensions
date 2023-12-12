using System;
using System.Diagnostics.CodeAnalysis;
using Dawn;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.Durable.Repository.Actions.Update;

[ExcludeFromCodeCoverage]
public class UpdateItemExecutionInfoInput : UpdateItemInput
{
    public UpdateItemExecutionInfoInput(Guid queueId, Guid itemId, RetryQueueItemStatus status, int attemptCount, DateTime lastExecution, string description)
        : base(itemId, status)
    {
            Guard.Argument(queueId, nameof(queueId)).NotDefault();
            Guard.Argument(attemptCount, nameof(attemptCount)).NotNegative();
            Guard.Argument(lastExecution, nameof(lastExecution)).NotDefault();

            this.QueueId = queueId;
            this.AttemptCount = attemptCount;
            this.LastExecution = lastExecution;
            this.Description = description;
        }

    public int AttemptCount { get; }
    public string Description { get; }
    public DateTime LastExecution { get; }
    public Guid QueueId { get; set; }
}