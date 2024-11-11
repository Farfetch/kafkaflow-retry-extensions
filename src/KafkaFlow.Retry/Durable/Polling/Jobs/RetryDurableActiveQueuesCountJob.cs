using System;
using System.Threading.Tasks;
using KafkaFlow.Retry.Durable.Definitions.Polling;
using KafkaFlow.Retry.Durable.Polling.Extensions;
using KafkaFlow.Retry.Durable.Repository;
using KafkaFlow.Retry.Durable.Repository.Actions.Delete;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Model;
using Quartz;

namespace KafkaFlow.Retry.Durable.Polling.Jobs;

[DisallowConcurrentExecution]
internal class RetryDurableActiveQueuesCountJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var jobDataMap = context.JobDetail.JobDataMap;

        var retryDurableActiveQueuesCountPollingDefinition =
            jobDataMap.GetValidValue<RetryDurableActiveQueuesCountPollingDefinition>(PollingJobConstants.RetryDurableActiveQueuesCountPollingDefinition,
                nameof(RetryDurableActiveQueuesCountJob));
        var schedulerId = jobDataMap.GetValidStringValue(PollingJobConstants.SchedulerId, nameof(RetryDurableActiveQueuesCountJob));
        var retryDurableQueueRepository =
            jobDataMap.GetValidValue<IRetryDurableQueueRepository>(PollingJobConstants.RetryDurableQueueRepository,
                nameof(RetryDurableActiveQueuesCountJob));
        var logHandler =
            jobDataMap.GetValidValue<ILogHandler>(PollingJobConstants.LogHandler, nameof(RetryDurableActiveQueuesCountJob));

        try
        {
            logHandler.Info(
                $"{nameof(RetryDurableActiveQueuesCountJob)} starts execution",
                new
                {
                    context.Trigger.Key.Name
                }
            );

            var countQueuesInput = new CountQueuesInput(RetryQueueStatus.Active)
            {
                SearchGroupKey = schedulerId
            };

            var countQueuesResult =
                await retryDurableQueueRepository.CountRetryQueuesAsync(countQueuesInput).ConfigureAwait(false);

            retryDurableActiveQueuesCountPollingDefinition.ActiveQueues(countQueuesResult);

            logHandler.Info(
                $"{nameof(RetryDurableActiveQueuesCountJob)} executed successfully.",
                new
                {
                    SearchGroupKey = schedulerId,
                    NumberOfActiveQueues = countQueuesResult
                });
        }
        catch (Exception ex)
        {
            logHandler.Error($"Exception on {nameof(RetryDurableActiveQueuesCountJob)} execution", ex, null);
        }
    }
}
