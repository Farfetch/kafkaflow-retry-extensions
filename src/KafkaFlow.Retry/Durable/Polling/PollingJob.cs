namespace KafkaFlow.Retry.Durable.Polling
{
    using System;
    using System.Threading.Tasks;
    using Dawn;
    using KafkaFlow.Producers;
    using KafkaFlow.Retry.Durable.Polling.Strategies;
    using KafkaFlow.Retry.Durable.Repository;
    using Quartz;

    [Quartz.DisallowConcurrentExecutionAttribute()]
    internal class PollingJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var jobDataMap = context.JobDetail.JobDataMap;

            Guard.Argument(jobDataMap.ContainsKey(PollingConstants.KafkaRetryDurableQueueRepository), PollingConstants.KafkaRetryDurableQueueRepository)
                .True("Argument KafkaRetryDurableQueueRepository wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(PollingConstants.KafkaRetryDurableProducer), PollingConstants.KafkaRetryDurableProducer)
                .True("Argument KafkaRetryDurableProducer wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(PollingConstants.KafkaRetryDurablePollingDefinition), PollingConstants.KafkaRetryDurablePollingDefinition)
                .True("Argument KafkaRetryDurablePollingDefinition wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(PollingConstants.KafkaRetryDurablePollingJobStrategy), PollingConstants.KafkaRetryDurablePollingJobStrategy)
                .True("Argument IPollingJobStrategyProvider wasn't found and is required for this job");

            var kafkaRetryDurableQueueRepository = jobDataMap[PollingConstants.KafkaRetryDurableQueueRepository] as IKafkaRetryDurableQueueRepository;
            var kafkaRetryDurableProducer = jobDataMap[PollingConstants.KafkaRetryDurableProducer] as IMessageProducer;
            var kafkaRetryDurablePollingDefinition = jobDataMap[PollingConstants.KafkaRetryDurablePollingDefinition] as KafkaRetryDurablePollingDefinition;
            var pollingJobStrategy = jobDataMap[PollingConstants.KafkaRetryDurablePollingJobStrategy] as IPollingJobStrategy;

            Guard.Argument(kafkaRetryDurableQueueRepository).NotNull();
            Guard.Argument(kafkaRetryDurableProducer).NotNull();
            Guard.Argument(kafkaRetryDurablePollingDefinition).NotNull();
            Guard.Argument(pollingJobStrategy).NotNull();

            try
            {
                await pollingJobStrategy
                    .ExecuteAsync(
                        kafkaRetryDurableQueueRepository,
                        kafkaRetryDurableProducer,
                        kafkaRetryDurablePollingDefinition)
                    .ConfigureAwait(false);
            }
            catch (KafkaRetryException ex)
            {
                // TODO: write a log for that
            }
            catch (Exception ex) // dispatch the handler and finish the job with success
            {
                // TODO: write a log for that
            }
        }
    }
}