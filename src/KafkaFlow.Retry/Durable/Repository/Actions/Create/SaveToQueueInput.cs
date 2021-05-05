namespace KafkaFlow.Retry.Durable.Repository.Actions.Create
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Dawn;
    using KafkaFlow.Retry.Durable.Common;
    using KafkaFlow.Retry.Durable.Repository.Model;

    [ExcludeFromCodeCoverage]
    public class SaveToQueueInput
    {
        public SaveToQueueInput(
            RetryQueueItemMessage message,
            string searchGroupKey,
            string queueGroupKey,
            RetryQueueStatus queueStatus,
            RetryQueueItemStatus itemStatus,
            SeverityLevel severity,
            DateTime creationDate,
            DateTime? lastExecution,
            DateTime? modifiedStatusDate,
            int attemptsCount,
            string description)
        {
            Guard.Argument(message, nameof(message)).NotNull();
            Guard.Argument(searchGroupKey, nameof(searchGroupKey)).NotNull().NotEmpty();
            Guard.Argument(queueGroupKey, nameof(queueGroupKey)).NotNull().NotEmpty();
            Guard.Argument(queueStatus, nameof(queueStatus)).NotDefault();
            Guard.Argument(itemStatus, nameof(itemStatus)).NotDefault();
            Guard.Argument(creationDate, nameof(creationDate)).NotDefault();
            Guard.Argument(modifiedStatusDate, nameof(modifiedStatusDate)).NotDefault();
            Guard.Argument(attemptsCount, nameof(attemptsCount)).NotNegative();

            this.Message = message;
            this.SearchGroupKey = searchGroupKey;
            this.QueueGroupKey = queueGroupKey;
            this.QueueStatus = queueStatus;
            this.ItemStatus = itemStatus;
            this.SeverityLevel = severity;
            this.CreationDate = creationDate;
            this.LastExecution = lastExecution;
            this.ModifiedStatusDate = modifiedStatusDate;
            this.AttemptsCount = attemptsCount;
            this.Description = description;
        }

        public int AttemptsCount { get; }
        public DateTime CreationDate { get; }
        public string Description { get; }
        public RetryQueueItemStatus ItemStatus { get; }
        public DateTime? LastExecution { get; }
        public RetryQueueItemMessage Message { get; }
        public DateTime? ModifiedStatusDate { get; }
        public string QueueGroupKey { get; }
        public RetryQueueStatus QueueStatus { get; }
        public string SearchGroupKey { get; }
        public SeverityLevel SeverityLevel { get; }
    }
}