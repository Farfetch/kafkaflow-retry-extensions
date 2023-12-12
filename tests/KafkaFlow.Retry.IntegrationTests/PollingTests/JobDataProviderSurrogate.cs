using System.Collections.Generic;
using KafkaFlow.Retry.Durable.Definitions.Polling;
using KafkaFlow.Retry.Durable.Polling;
using Quartz;

namespace KafkaFlow.Retry.IntegrationTests.PollingTests;

internal class JobDataProviderSurrogate : IJobDataProvider
{
    public JobDataProviderSurrogate(string schedulerId, PollingDefinition pollingDefinition, ITrigger trigger,
        List<IJobExecutionContext> jobExecutionContexts)
    {
        PollingDefinition = pollingDefinition;

        Trigger = trigger;
        TriggerName = GetTriggerName(schedulerId);

        JobExecutionContexts = jobExecutionContexts;
        JobDetail = CreateJobDetail();
    }

    public List<IJobExecutionContext> JobExecutionContexts { get; }

    public string TriggerName { get; }

    public IJobDetail JobDetail { get; }

    public PollingDefinition PollingDefinition { get; }

    public ITrigger Trigger { get; }

    private IJobDetail CreateJobDetail()
    {
        var dataMap = new JobDataMap { { "JobExecution", JobExecutionContexts } };

        return JobBuilder
            .Create<JobSurrogate>()
            .SetJobData(dataMap)
            .Build();
    }

    private string GetTriggerName(string schedulerId)
    {
        return $"pollingJobTrigger_{schedulerId}_{PollingDefinition.PollingJobType}";
    }
}