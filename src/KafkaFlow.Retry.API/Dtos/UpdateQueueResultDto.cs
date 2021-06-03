namespace KafkaFlow.Retry.API.Dtos
{
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;
    using KafkaFlow.Retry.Durable.Repository.Model;

    public class UpdateQueueResultDto
    {
        public UpdateQueueResultDto(string queueGroupKey, UpdateQueueResultStatus status, RetryQueueStatus retryQueueStatus)
        {
            this.QueueGroupKey = queueGroupKey;
            this.Result = status.ToString();
            this.QueueStatus = retryQueueStatus.ToString();
        }

        public string QueueGroupKey { get; set; }

        public string QueueStatus { get; set; }

        public string Result { get; set; }
    }
}