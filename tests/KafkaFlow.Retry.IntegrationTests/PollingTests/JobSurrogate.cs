using System.Collections.Generic;
using System.Threading.Tasks;
using Quartz;

namespace KafkaFlow.Retry.IntegrationTests.PollingTests;

internal class JobSurrogate : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
            var jobExecutionContexts = context.JobDetail.JobDataMap["JobExecution"] as List<IJobExecutionContext>;
            jobExecutionContexts.Add(context);

            return Task.CompletedTask;
        }
}