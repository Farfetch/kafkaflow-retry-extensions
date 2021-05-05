namespace KafkaFlow.Retry.MongoDb
{
    using KafkaFlow.Retry.Durable.Repository;

    public class DataProviderCreationResult
    {
        internal DataProviderCreationResult(string message, IRetryQueueDataProvider result, bool success)
        {
            this.Message = message;
            this.Result = result;
            this.Success = success;
        }

        public string Message { get; }
        public IRetryQueueDataProvider Result { get; }
        public bool Success { get; }
    }
}