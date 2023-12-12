using System;
using System.Threading.Tasks;
using KafkaFlow.Retry.Durable.Definitions.Polling;
using KafkaFlow.Retry.Durable.Polling.Extensions;
using KafkaFlow.Retry.Durable.Repository;
using KafkaFlow.Retry.Durable.Repository.Actions.Delete;
using KafkaFlow.Retry.Durable.Repository.Model;
using Quartz;

namespace KafkaFlow.Retry.Durable.Polling.Jobs;

[DisallowConcurrentExecutionAttribute]
internal class CleanupPollingJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var jobDataMap = context.JobDetail.JobDataMap;

        var cleanupPollingDefinition =
            jobDataMap.GetValidValue<CleanupPollingDefinition>(PollingJobConstants.CleanupPollingDefinition,
                nameof(CleanupPollingJob));
        var schedulerId = jobDataMap.GetValidStringValue(PollingJobConstants.SchedulerId, nameof(CleanupPollingJob));
        var retryDurableQueueRepository =
            jobDataMap.GetValidValue<IRetryDurableQueueRepository>(PollingJobConstants.RetryDurableQueueRepository,
                nameof(CleanupPollingJob));
        var logHandler =
            jobDataMap.GetValidValue<ILogHandler>(PollingJobConstants.LogHandler, nameof(CleanupPollingJob));

        try
        {
            logHandler.Info(
                $"{nameof(CleanupPollingJob)} starts execution",
                new
                {
                    context.Trigger.Key.Name
                }
            );

            var maxLastExecutionDateToBeKept = DateTime.UtcNow.AddDays(-1 * cleanupPollingDefinition.TimeToLiveInDays);

            var deleteQueuesInput = new DeleteQueuesInput(
                schedulerId,
                RetryQueueStatus.Done,
                maxLastExecutionDateToBeKept,
                cleanupPollingDefinition.RowsPerRequest);

            var deleteQueuesResult =
                await retryDurableQueueRepository.DeleteQueuesAsync(deleteQueuesInput).ConfigureAwait(false);

            var logObj = new
            {
                cleanupPollingDefinition.TimeToLiveInDays,
                MaxLastExecutionDateToBeKept = maxLastExecutionDateToBeKept,
                MaxQueuesThatCanBeDeleted = cleanupPollingDefinition.RowsPerRequest,
                deleteQueuesResult.TotalQueuesDeleted
            };

            logHandler.Info($"{nameof(CleanupPollingJob)} executed successfully.", logObj);
        }
        catch (Exception ex)
        {
            logHandler.Error($"Exception on {nameof(CleanupPollingJob)} execution", ex, null);
        }
    }
}