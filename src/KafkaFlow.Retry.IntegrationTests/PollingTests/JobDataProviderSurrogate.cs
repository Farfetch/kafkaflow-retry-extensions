namespace KafkaFlow.Retry.IntegrationTests.PollingTests
{
    using System.Collections.Generic;
    using global::KafkaFlow.Retry.Durable.Definitions.Polling;
    using global::KafkaFlow.Retry.Durable.Polling;
    using Quartz;

    internal class JobDataProviderSurrogate : IJobDataProvider
    {
        public JobDataProviderSurrogate(string schedulerId, PollingDefinition pollingDefinition, ITrigger trigger, List<IJobExecutionContext> jobExecutionContexts)
        {
            this.PollingDefinition = pollingDefinition;

            this.Trigger = trigger;
            this.TriggerName = this.GetTriggerName(schedulerId);

            this.JobExecutionContexts = jobExecutionContexts;
            this.JobDetail = this.CreateJobDetail();
        }

        public IJobDetail JobDetail { get; }

        public List<IJobExecutionContext> JobExecutionContexts { get; }

        public PollingDefinition PollingDefinition { get; }

        public ITrigger Trigger { get; }

        public string TriggerName { get; }

        private IJobDetail CreateJobDetail()
        {
            var dataMap = new JobDataMap { { "JobExecution", this.JobExecutionContexts } };

            return JobBuilder
                    .Create<JobSurrogate>()
                    .SetJobData(dataMap)
                    .Build();
        }

        private string GetTriggerName(string schedulerId)
        {
            return $"pollingJobTrigger_{schedulerId}_{this.PollingDefinition.PollingJobType}";
        }
    }
}
