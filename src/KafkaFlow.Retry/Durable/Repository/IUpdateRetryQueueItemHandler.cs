using System.Threading.Tasks;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;

namespace KafkaFlow.Retry.Durable.Repository;

internal interface IUpdateRetryQueueItemHandler
{
    bool CanHandle(UpdateItemInput input);

    Task UpdateItemAsync(UpdateItemInput input);
}