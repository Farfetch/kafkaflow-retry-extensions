namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Polling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable.Definitions.Polling;
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
        public async Task QueueTracker_ScheduleAndUnscheduleDifferentJobs_Success()
        {
            // arrange
            var schedulerId = "twoJobsSchedulerId";

            var retryDurablePollingDefinition =
               new RetryDurablePollingDefinition(
                   enabled: true,
                   cronExpression: "*/2 * * ? * * *",
                   fetchSize: 100,
                   expirationIntervalFactor: 1
               );

            var cleanupPollingDefinition =
               new CleanupPollingDefinition(
                   enabled: true,
                   cronExpression: "*/4 * * ? * * *",
                   timeToLiveInDays: 1,
                   rowsPerRequest: 10
               );

            var retryDurableTriggerKeyName = $"Trigger_{schedulerId}_{retryDurablePollingDefinition.PollingJobType}";
            var cleanupTriggerKeyName = $"Trigger_{schedulerId}_{cleanupPollingDefinition.PollingJobType}";

            var jobExecutionContexts = new List<IJobExecutionContext>();

            var mockRetryDurableJobDataProvider = new Mock<IJobDataProvider>();
            this.SetupMockJobDataProvider(
                mockRetryDurableJobDataProvider,
                retryDurablePollingDefinition,
                retryDurableTriggerKeyName,
                jobExecutionContexts);

            var mockCleanupJobDataProvider = new Mock<IJobDataProvider>();
            this.SetupMockJobDataProvider(
                mockCleanupJobDataProvider,
                cleanupPollingDefinition,
                cleanupTriggerKeyName,
                jobExecutionContexts);

            var queueTracker = new QueueTracker(
              schedulerId,
              new[] { mockRetryDurableJobDataProvider.Object, mockCleanupJobDataProvider.Object },
              Mock.Of<ILogHandler>()
              );

            // act
            await queueTracker.ScheduleJobsAsync();

            await WaitForSeconds(5);

            await queueTracker.UnscheduleJobsAsync();

            // assert
            jobExecutionContexts.Where(x => x.PreviousFireTimeUtc is null).Count().Should().Be(2);
            jobExecutionContexts.Where(x => x.PreviousFireTimeUtc is null && x.Trigger.Key.Name == retryDurableTriggerKeyName).Count().Should().Be(1);
            jobExecutionContexts.Where(x => x.PreviousFireTimeUtc is null && x.Trigger.Key.Name == cleanupTriggerKeyName).Count().Should().Be(1);
        }

        [Fact]
        public async Task QueueTracker_ScheduleAndUnscheduleRetryDurableJobs_Success()
        {
            // arrange
            var schedulerId = "pollingId";

            var retryDurablePollingDefinition =
                new RetryDurablePollingDefinition(
                    enabled: true,
                    cronExpression: "*/5 * * ? * * *",
                    fetchSize: 100,
                    expirationIntervalFactor: 1
                );

            var jobExecutionContexts = new List<IJobExecutionContext>();
            var dataMap = new JobDataMap { { "JobExecution", jobExecutionContexts } };

            var mockIJobDataProvider = new Mock<IJobDataProvider>();
            mockIJobDataProvider
                .Setup(x => x.JobDetail)
                .Returns(
                    JobBuilder
                    .Create<MockIJob>()
                    .SetJobData(dataMap)
                    .Build()
                );

            mockIJobDataProvider
                .SetupGet(m => m.PollingDefinition)
                .Returns(retryDurablePollingDefinition);

            mockIJobDataProvider
                .SetupGet(m => m.Trigger)
                .Returns(
                    TriggerBuilder
                        .Create()
                        .WithIdentity("Trigger1", "queueTrackerGroup")
                        .WithCronSchedule("*/5 * * ? * * *")
                        .StartNow()
                        .WithPriority(1)
                        .Build());

            var mockILogHandler = new Mock<ILogHandler>();
            mockILogHandler.Setup(x => x.Info(It.IsAny<string>(), It.IsAny<object>()));
            mockILogHandler.Setup(x => x.Error(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<object>()));

            var queueTracker1 = new QueueTracker(
                schedulerId,
                new[] { mockIJobDataProvider.Object },
                mockILogHandler.Object
                );

            // act
            await queueTracker1.ScheduleJobsAsync();

            await WaitForSeconds(6).ConfigureAwait(false);

            await queueTracker1.UnscheduleJobsAsync();

            await WaitForSeconds(15).ConfigureAwait(false);

            mockIJobDataProvider
                .SetupGet(m => m.Trigger)
                .Returns(
                    TriggerBuilder
                        .Create()
                        .WithIdentity("Trigger2", "queueTrackerGroup")
                        .WithCronSchedule("*/5 * * ? * * *")
                        .StartNow()
                        .WithPriority(1)
                        .Build());

            var queueTracker2 = new QueueTracker(
                schedulerId,
                new[] { mockIJobDataProvider.Object },
                mockILogHandler.Object
                );

            await queueTracker2.ScheduleJobsAsync();

            await WaitForSeconds(6).ConfigureAwait(false);

            await queueTracker2.UnscheduleJobsAsync();

            await WaitForSeconds(15).ConfigureAwait(false);

            jobExecutionContexts.Where(x => x.PreviousFireTimeUtc is null).Count().Should().Be(2);
            jobExecutionContexts.Where(x => x.PreviousFireTimeUtc is null && x.Trigger.Key.Name == "Trigger1").Count().Should().Be(1);
            jobExecutionContexts.Where(x => x.PreviousFireTimeUtc is null && x.Trigger.Key.Name == "Trigger2").Count().Should().Be(1);

            var timeBetweenJobExecutionsWhileJobWasUnscheduled =
                jobExecutionContexts
                    .Where(x => x.Trigger.Key.Name == "Trigger2")
                    .OrderBy(x => x.FireTimeUtc)
                    .First()
                    .FireTimeUtc
                    -
                jobExecutionContexts
                    .Where(x => x.Trigger.Key.Name == "Trigger1")
                    .OrderBy(x => x.FireTimeUtc)
                    .Last()
                    .FireTimeUtc;

            timeBetweenJobExecutionsWhileJobWasUnscheduled.Should().BeGreaterThan(TimeSpan.FromSeconds(15));
        }

        private static async Task WaitForSeconds(int seconds)
        {
            for (int i = 0; i < (seconds * 10); i++)
            {
                await Task.Delay(100).ConfigureAwait(false);
            }
        }

        private void SetupMockJobDataProvider(
            Mock<IJobDataProvider> mockJobDataProvider,
            PollingDefinition pollingDefinition,
            string triggerKeyName,
            List<IJobExecutionContext> jobExecutionContexts)
        {
            var dataMap = new JobDataMap { { "JobExecution", jobExecutionContexts } };

            mockJobDataProvider
                .Setup(x => x.JobDetail)
                .Returns(
                    JobBuilder
                    .Create<MockIJob>()
                    .SetJobData(dataMap)
                    .Build()
                );

            mockJobDataProvider
                .SetupGet(m => m.PollingDefinition)
                .Returns(pollingDefinition);

            mockJobDataProvider
                .SetupGet(m => m.Trigger)
                .Returns(
                    TriggerBuilder
                        .Create()
                        .WithIdentity(triggerKeyName, "queueTrackerGroupTest")
                        .WithCronSchedule(pollingDefinition.CronExpression)
                        .StartNow()
                        .WithPriority(1)
                        .Build());
        }
    }
}