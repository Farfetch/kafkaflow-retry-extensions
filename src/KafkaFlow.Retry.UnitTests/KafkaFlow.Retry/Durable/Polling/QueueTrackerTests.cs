namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Polling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable.Definitions;
    using global::KafkaFlow.Retry.Durable.Polling;
    using Moq;
    using Quartz;
    using Xunit;

    public class MockIJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var jobExecutionContexts = context.JobDetail.JobDataMap["JobExecution"] as List<IJobExecutionContext>;
            jobExecutionContexts.Add(context);

            return Task.CompletedTask;
        }
    }

    public class QueueTrackerTests
    {
        [Fact]
        public async Task QueueTracker_Create_Success()
        {
            // arrange
            var mockITriggerProvider = new Mock<ITriggerProvider>();
            mockITriggerProvider
                .Setup(x => x.GetQueuePollingTrigger())
                .Returns(() =>
                    TriggerBuilder
                    .Create()
                    .WithIdentity("Trigger", "queueTrackerGroup")
                    .WithCronSchedule("*/5 * * ? * * *")
                    .StartNow()
                    .WithPriority(1)
                    .Build()
                );

            IList<IJobExecutionContext> jobExecutionContexts = new List<IJobExecutionContext>();
            JobDataMap dataMap = new JobDataMap();
            dataMap.Add("JobExecution", jobExecutionContexts);
            var mockIJobDetailProvider = new Mock<IJobDetailProvider>();
            mockIJobDetailProvider
                .Setup(x => x.GetQueuePollingJobDetail())
                .Returns(
                    JobBuilder
                    .Create<MockIJob>()
                    .SetJobData(dataMap)
                    .Build()
                );

            var mockILogHandler = new Mock<ILogHandler>();
            mockILogHandler.Setup(x => x.Info(It.IsAny<string>(), It.IsAny<object>()));
            mockILogHandler.Setup(x => x.Error(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<object>()));
            var retryDurablePollingDefinition =
                new RetryDurablePollingDefinition(
                    enabled: true,
                    cronExpression: "*/5 * * ? * * *",
                    fetchSize: 100,
                    expirationIntervalFactor: 1,
                    id: "pollingId"
                );

            var queueTracker = new QueueTracker(
                mockILogHandler.Object,
                retryDurablePollingDefinition,
                mockIJobDetailProvider.Object,
                mockITriggerProvider.Object
                );

            // act
            queueTracker.ScheduleJob();

            await WaitForSeconds(6).ConfigureAwait(false);

            queueTracker.UnscheduleJob();

            await WaitForSeconds(15).ConfigureAwait(false);

            var mockITriggerProvider1 = new Mock<ITriggerProvider>();
            mockITriggerProvider1
                .Setup(x => x.GetQueuePollingTrigger())
                .Returns(() =>
                    TriggerBuilder
                    .Create()
                    .WithIdentity("Trigger1", "queueTrackerGroup")
                    .WithCronSchedule("*/5 * * ? * * *")
                    .StartNow()
                    .WithPriority(1)
                    .Build()
                );

            var queueTracker1 = new QueueTracker(
                mockILogHandler.Object,
                retryDurablePollingDefinition,
                mockIJobDetailProvider.Object,
                mockITriggerProvider1.Object
                );

            queueTracker1.ScheduleJob();

            await WaitForSeconds(6).ConfigureAwait(false);

            queueTracker1.UnscheduleJob();

            await WaitForSeconds(15).ConfigureAwait(false);

            jobExecutionContexts.Where(x => x.PreviousFireTimeUtc is null).Count().Should().Be(2);
            jobExecutionContexts.Where(x => x.PreviousFireTimeUtc is null && x.Trigger.Key.Name == "Trigger").Count().Should().Be(1);
            jobExecutionContexts.Where(x => x.PreviousFireTimeUtc is null && x.Trigger.Key.Name == "Trigger1").Count().Should().Be(1);
            (
                jobExecutionContexts
                    .Where(x => x.Trigger.Key.Name == "Trigger1")
                    .OrderBy(x => x.FireTimeUtc)
                    .First()
                    .FireTimeUtc
                    -
                jobExecutionContexts
                    .Where(x => x.Trigger.Key.Name == "Trigger")
                    .OrderBy(x => x.FireTimeUtc)
                    .Last()
                    .FireTimeUtc
            ).Should().BeGreaterThan(TimeSpan.FromSeconds(15));
        }

        private static async Task WaitForSeconds(int seconds)
        {
            for (int i = 0; i < (seconds * 10); i++)
            {
                await Task.Delay(100).ConfigureAwait(false);
            }
        }
    }
}