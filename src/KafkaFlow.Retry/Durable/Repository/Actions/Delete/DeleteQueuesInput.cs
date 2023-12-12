using System;
using Dawn;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.Durable.Repository.Actions.Delete;

public class DeleteQueuesInput
{
    public DeleteQueuesInput(
        string searchGroupKey,
        RetryQueueStatus retryQueueStatus,
        DateTime maxLastExecutionDateToBeKept,
        int maxRowsToDelete)
    {
            Guard.Argument(searchGroupKey, nameof(searchGroupKey)).NotNull().NotEmpty();
            Guard.Argument(retryQueueStatus, nameof(retryQueueStatus)).NotDefault();
            Guard.Argument(maxLastExecutionDateToBeKept, nameof(maxLastExecutionDateToBeKept)).NotDefault();
            Guard.Argument(maxRowsToDelete, nameof(maxRowsToDelete)).Positive();

            this.SearchGroupKey = searchGroupKey;
            this.RetryQueueStatus = retryQueueStatus;
            this.MaxLastExecutionDateToBeKept = maxLastExecutionDateToBeKept;
            this.MaxRowsToDelete = maxRowsToDelete;
        }

    public DateTime MaxLastExecutionDateToBeKept { get; }

    public int MaxRowsToDelete { get; }

    public RetryQueueStatus RetryQueueStatus { get; }

    public string SearchGroupKey { get; }
}