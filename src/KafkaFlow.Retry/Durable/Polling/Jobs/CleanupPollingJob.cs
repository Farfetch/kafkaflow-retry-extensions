namespace KafkaFlow.Retry.Durable.Polling.Jobs
{
    using System;
    using System.Threading.Tasks;
    using Dawn;
    using KafkaFlow.Retry.Durable.Definitions.Polling;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Repository.Actions.Delete;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using Quartz;

    [DisallowConcurrentExecutionAttribute]
    internal class CleanupPollingJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var jobDataMap = context.JobDetail.JobDataMap;

            Guard.Argument(jobDataMap.ContainsKey(PollingJobConstants.RetryDurableQueueRepository), PollingJobConstants.RetryDurableQueueRepository)
                .True("Argument RetryDurableQueueRepository wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(PollingJobConstants.CleanupPollingDefinition), PollingJobConstants.CleanupPollingDefinition)
                .True("Argument CleanupPollingDefinition wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(PollingJobConstants.LogHandler), PollingJobConstants.LogHandler)
                .True("Argument LogHandler wasn't found and is required for this job");

            var retryDurableQueueRepository = jobDataMap[PollingJobConstants.RetryDurableQueueRepository] as IRetryDurableQueueRepository;
            var cleanupPollingDefinition = jobDataMap[PollingJobConstants.CleanupPollingDefinition] as CleanupPollingDefinition;
            var logHandler = jobDataMap[PollingJobConstants.LogHandler] as ILogHandler;
            var schedulerId = jobDataMap[PollingJobConstants.SchedulerId] as string;

            Guard.Argument(retryDurableQueueRepository).NotNull();
            Guard.Argument(cleanupPollingDefinition).NotNull();
            Guard.Argument(logHandler).NotNull();
            Guard.Argument(schedulerId).NotNull().NotEmpty();

            try
            {
                logHandler.Info(
                    $"{nameof(CleanupPollingJob)} starts execution",
                    new
                    {
                        Name = context.Trigger.Key.Name
                    }
                );

                var maxLastExecutionDateToBeKept = DateTime.UtcNow.AddDays(-1 * cleanupPollingDefinition.TimeToLiveInDays);

                var deleteQueuesInput = new DeleteQueuesInput(
                    schedulerId,
                    RetryQueueStatus.Done,
                    maxLastExecutionDateToBeKept,
                    cleanupPollingDefinition.RowsPerRequest);

                var deleteQueuesResult = await retryDurableQueueRepository.DeleteQueuesAsync(deleteQueuesInput).ConfigureAwait(false);

                var logObj = new
                {
                    TimeToLiveInDays = cleanupPollingDefinition.TimeToLiveInDays,
                    MaxLastExecutionDateToBeKept = maxLastExecutionDateToBeKept,
                    MaxQueuesThatCanBeDeleted = cleanupPollingDefinition.RowsPerRequest,
                    TotalQueuesDeleted = deleteQueuesResult.TotalQueuesDeleted
                };

                logHandler.Info($"{nameof(CleanupPollingJob)} executed successfully.", logObj);
            }
            catch (Exception ex)
            {
                logHandler.Error($"Exception on {nameof(CleanupPollingJob)} execution", ex, null);
            }
        }
    }
}