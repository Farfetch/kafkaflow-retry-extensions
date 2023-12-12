using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using KafkaFlow.Retry.Durable.Definitions.Polling;
using KafkaFlow.Retry.Durable.Polling;
using Moq;
using Quartz;

namespace KafkaFlow.Retry.IntegrationTests.PollingTests;

public class QueueTrackerCoordinatorTests
{
    private readonly Mock<IJobDataProvidersFactory> _mockJobDataProvidersFactory;
    private readonly ITriggerProvider _triggerProvider;

    public QueueTrackerCoordinatorTests()
    {
            _triggerProvider = new TriggerProvider();

            _mockJobDataProvidersFactory = new Mock<IJobDataProvidersFactory>();
        }

    [Fact]
    public async Task QueueTrackerCoordinator_ForceMisfireJob_SuccessWithCorrectScheduledFiredTimes()
    {
            // arrange
            var schedulerId = "MisfiredJobsDoesNothing";
            var jobExecutionContexts = new List<IJobExecutionContext>();

            var waitForScheduleInSeconds = 5;
            var jobActiveTimeInSeconds = 8;
            var pollingInSeconds = 2;

            var cronExpression = $"*/{pollingInSeconds} * * ? * * *";

            var retryDurableJobDataProvider = CreateRetryDurableJobDataProvider(schedulerId, cronExpression, jobExecutionContexts);

            _mockJobDataProvidersFactory
               .Setup(m => m.Create(It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()))
               .Returns(new[] { retryDurableJobDataProvider });

            var queueTrackerCoordinator = CreateQueueTrackerCoordinator(schedulerId);

            // act

            Thread.Sleep(waitForScheduleInSeconds * 1000);

            await queueTrackerCoordinator.ScheduleJobsAsync(Mock.Of<IMessageProducer>(), Mock.Of<ILogHandler>());

            Thread.Sleep(jobActiveTimeInSeconds * 1000);

            await queueTrackerCoordinator.UnscheduleJobsAsync();

            // assert
            var scheduledFiredTimes = jobExecutionContexts
                .Where(ctx => ctx.ScheduledFireTimeUtc.HasValue)
                .Select(ctx => ctx.ScheduledFireTimeUtc.Value)
                .OrderBy(x => x)
                .ToList();

            var currentScheduledFiredTime = scheduledFiredTimes.First();
            var otherScheduledFiredTimes = scheduledFiredTimes.Skip(1).ToList();

            foreach (var scheduledFiredTime in otherScheduledFiredTimes)
            {
                currentScheduledFiredTime.AddSeconds(pollingInSeconds).Should().Be(scheduledFiredTime);

                currentScheduledFiredTime = scheduledFiredTime;
            }
        }

    [Fact]
    public async Task QueueTrackerCoordinator_ScheduleAndUnscheduleDifferentJobs_Success()
    {
            // arrange
            var schedulerId = "twoJobsSchedulerId";
            var jobExecutionContexts = new List<IJobExecutionContext>();

            var timePollingActiveInSeconds = 4;

            var retryDurableCronExpression = "*/2 * * ? * * *";
            var cleanupCronExpression = "*/4 * * ? * * *";

            var retryDurableMinExpectedJobsFired = 2;
            var retryDurableMaxExpectedJobsFired = 3;
            var cleanupMinExpectedJobsFired = 1;
            var cleanupMaxExpectedJobsFired = 2;

            var retryDurableJobDataProvider = CreateRetryDurableJobDataProvider(schedulerId, retryDurableCronExpression, jobExecutionContexts);
            var cleanupJobDataProvider = CreateCleanupJobDataProvider(schedulerId, cleanupCronExpression, jobExecutionContexts);

            _mockJobDataProvidersFactory
                .Setup(m => m.Create(It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()))
                .Returns(new[] { retryDurableJobDataProvider, cleanupJobDataProvider });

            var queueTrackerCoordinator = CreateQueueTrackerCoordinator(schedulerId);

            // act
            await queueTrackerCoordinator.ScheduleJobsAsync(Mock.Of<IMessageProducer>(), Mock.Of<ILogHandler>());

            Thread.Sleep(timePollingActiveInSeconds * 1000);

            await queueTrackerCoordinator.UnscheduleJobsAsync();

            // assert
            jobExecutionContexts.Where(ctx => !ctx.PreviousFireTimeUtc.HasValue).Should().HaveCount(2);

            var retryDurableFiresContexts = jobExecutionContexts.Where(ctx => ctx.Trigger.Key.Name == retryDurableJobDataProvider.TriggerName);
            var cleanupFiresContexts = jobExecutionContexts.Where(ctx => ctx.Trigger.Key.Name == cleanupJobDataProvider.TriggerName);

            retryDurableFiresContexts
                .Should()
                .HaveCountGreaterThanOrEqualTo(retryDurableMinExpectedJobsFired)
                .And
                .HaveCountLessThanOrEqualTo(retryDurableMaxExpectedJobsFired);

            retryDurableFiresContexts.Should().ContainSingle(ctx => !ctx.PreviousFireTimeUtc.HasValue);

            cleanupFiresContexts
                .Should()
                .HaveCountGreaterThanOrEqualTo(cleanupMinExpectedJobsFired)
                .And
                .HaveCountLessThanOrEqualTo(cleanupMaxExpectedJobsFired);

            cleanupFiresContexts.Should().ContainSingle(ctx => !ctx.PreviousFireTimeUtc.HasValue);
        }

    private JobDataProviderSurrogate CreateCleanupJobDataProvider(string schedulerId, string cronExpression, List<IJobExecutionContext> jobExecutionContexts)
    {
            var cleanupPollingDefinition =
               new CleanupPollingDefinition(
                   enabled: true,
                   cronExpression: cronExpression,
                   timeToLiveInDays: 1,
                   rowsPerRequest: 10
               );

            return CreateJobDataProvider(schedulerId, cleanupPollingDefinition, jobExecutionContexts);
        }

    private JobDataProviderSurrogate CreateJobDataProvider(string schedulerId, PollingDefinition pollingDefinition, List<IJobExecutionContext> jobExecutionContexts)
    {
            var trigger = _triggerProvider.GetPollingTrigger(schedulerId, pollingDefinition);

            return new JobDataProviderSurrogate(schedulerId, pollingDefinition, trigger, jobExecutionContexts);
        }

    private IQueueTrackerCoordinator CreateQueueTrackerCoordinator(string schedulerId)
    {
            var queueTrackerFactory = new QueueTrackerFactory(schedulerId, _mockJobDataProvidersFactory.Object);

            return new QueueTrackerCoordinator(queueTrackerFactory);
        }

    private JobDataProviderSurrogate CreateRetryDurableJobDataProvider(string schedulerId, string cronExpression, List<IJobExecutionContext> jobExecutionContexts)
    {
            var retryDurablePollingDefinition =
               new RetryDurablePollingDefinition(
                   enabled: true,
                   cronExpression: cronExpression,
                   fetchSize: 100,
                   expirationIntervalFactor: 1
               );

            return CreateJobDataProvider(schedulerId, retryDurablePollingDefinition, jobExecutionContexts);
        }
}