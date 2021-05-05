namespace KafkaFlow.Retry.Durable.Repository
{
    using System.Threading.Tasks;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;

    internal interface IUpdateRetryQueueItemHandler
    {
        bool CanHandle(UpdateItemInput input);

        Task UpdateItemAsync(UpdateItemInput input);
    }
}
