using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using KafkaFlow.Retry.Durable.Definitions.Polling;
using KafkaFlow.Retry.Durable.Polling;
using Moq;
using Quartz;
using Xunit.Abstractions;

namespace KafkaFlow.Retry.IntegrationTests.PollingTests;

public class QueueTrackerCoordinatorTests
{
    private readonly Mock<IJobDataProvidersFactory> _mockJobDataProvidersFactory;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ITriggerProvider _triggerProvider;

    public QueueTrackerCoordinatorTests(ITestOutputHelper testOutputHelper)
    {
        _triggerProvider = new TriggerProvider();

        _mockJobDataProvidersFactory = new Mock<IJobDataProvidersFactory>();
        _testOutputHelper = testOutputHelper;
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

        var retryDurableJobDataProvider =
            CreateRetryDurableJobDataProvider(schedulerId, cronExpression, jobExecutionContexts);

        _mockJobDataProvidersFactory
            .Setup(m => m.Create(It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()))
            .Returns(new[] { retryDurableJobDataProvider });

        var queueTrackerCoordinator = CreateQueueTrackerCoordinator(schedulerId);

        // act
        await Task.Delay(waitForScheduleInSeconds * 1000);

        await queueTrackerCoordinator.ScheduleJobsAsync(Mock.Of<IMessageProducer>(), Mock.Of<ILogHandler>());

        await Task.Delay(jobActiveTimeInSeconds * 1000);

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

        var timePollingActiveInSeconds = 60;

        var retryDurableCronExpression = "0/2 * * ? * * *";
        var cleanupCronExpression = "0/2 * * ? * * *";

        var retryDurableMinExpectedJobsFired = 2;
        var retryDurableMaxExpectedJobsFired = 4;
        var cleanupMinExpectedJobsFired = 2;
        var cleanupMaxExpectedJobsFired = 4;

        var retryDurableJobDataProvider =
            CreateRetryDurableJobDataProvider(schedulerId, retryDurableCronExpression, jobExecutionContexts);
        var cleanupJobDataProvider =
            CreateCleanupJobDataProvider(schedulerId, cleanupCronExpression, jobExecutionContexts);

        _mockJobDataProvidersFactory
            .Setup(m => m.Create(It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()))
            .Returns(new[] { retryDurableJobDataProvider, cleanupJobDataProvider });

        var queueTrackerCoordinator = CreateQueueTrackerCoordinator(schedulerId);

        // act
        await queueTrackerCoordinator.ScheduleJobsAsync(Mock.Of<IMessageProducer>(), Mock.Of<ILogHandler>());

        for (int i = 0; i < 60; i++)
        {
            await Task.Delay(100);
        }

        await queueTrackerCoordinator.UnscheduleJobsAsync();

        // assert
        _testOutputHelper.WriteLine($"JobExecutionContexts:\n{string.Join('\n', jobExecutionContexts.Select(s => $"PreviousFireTimeUtc = {s.PreviousFireTimeUtc} TriggerName = {s.Trigger.Key.Name}"))}");

        var retryDurableFiresContexts =
            jobExecutionContexts.Where(ctx => ctx.Trigger.Key.Name == retryDurableJobDataProvider.TriggerName);
        var cleanupFiresContexts =
            jobExecutionContexts.Where(ctx => ctx.Trigger.Key.Name == cleanupJobDataProvider.TriggerName);

        retryDurableFiresContexts
            .Should()
            .HaveCountGreaterThanOrEqualTo(retryDurableMinExpectedJobsFired)
            .And
            .HaveCountLessThanOrEqualTo(retryDurableMaxExpectedJobsFired);

        cleanupFiresContexts
            .Should()
            .HaveCountGreaterThanOrEqualTo(cleanupMinExpectedJobsFired)
            .And
            .HaveCountLessThanOrEqualTo(cleanupMaxExpectedJobsFired);
    }

    private JobDataProviderSurrogate CreateCleanupJobDataProvider(string schedulerId, string cronExpression,
        List<IJobExecutionContext> jobExecutionContexts)
    {
        var cleanupPollingDefinition =
            new CleanupPollingDefinition(
                true,
                cronExpression,
                1,
                10
            );

        return CreateJobDataProvider(schedulerId, cleanupPollingDefinition, jobExecutionContexts);
    }

    private JobDataProviderSurrogate CreateJobDataProvider(string schedulerId, PollingDefinition pollingDefinition,
        List<IJobExecutionContext> jobExecutionContexts)
    {
        var trigger = _triggerProvider.GetPollingTrigger(schedulerId, pollingDefinition);

        return new JobDataProviderSurrogate(schedulerId, pollingDefinition, trigger, jobExecutionContexts);
    }

    private IQueueTrackerCoordinator CreateQueueTrackerCoordinator(string schedulerId)
    {
        var queueTrackerFactory = new QueueTrackerFactory(schedulerId, _mockJobDataProvidersFactory.Object);

        return new QueueTrackerCoordinator(queueTrackerFactory);
    }

    private JobDataProviderSurrogate CreateRetryDurableJobDataProvider(string schedulerId, string cronExpression,
        List<IJobExecutionContext> jobExecutionContexts)
    {
        var retryDurablePollingDefinition =
            new RetryDurablePollingDefinition(
                true,
                cronExpression,
                100,
                1
            );

        return CreateJobDataProvider(schedulerId, retryDurablePollingDefinition, jobExecutionContexts);
    }
}