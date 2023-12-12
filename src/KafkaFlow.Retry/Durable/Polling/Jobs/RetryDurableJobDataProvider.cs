using Dawn;
using KafkaFlow.Retry.Durable.Definitions.Polling;
using KafkaFlow.Retry.Durable.Encoders;
using KafkaFlow.Retry.Durable.Repository;
using KafkaFlow.Retry.Durable.Repository.Adapters;
using Quartz;

namespace KafkaFlow.Retry.Durable.Polling.Jobs;

internal class RetryDurableJobDataProvider : IJobDataProvider
{
    private readonly IJobDetail jobDetail;
    private readonly RetryDurablePollingDefinition retryDurablePollingDefinition;
    private readonly ITrigger trigger;

    public RetryDurableJobDataProvider(
        RetryDurablePollingDefinition retryDurablePollingDefinition,
        ITrigger trigger,
        string schedulerId,
        IRetryDurableQueueRepository retryDurableQueueRepository,
        ILogHandler logHandler,
        IMessageHeadersAdapter messageHeadersAdapter,
        IUtf8Encoder utf8Encoder,
        IMessageProducer retryDurableMessageProducer)
    {
            Guard.Argument(retryDurablePollingDefinition, nameof(retryDurablePollingDefinition)).NotNull();
            Guard.Argument(trigger, nameof(trigger)).NotNull();
            Guard.Argument(schedulerId, nameof(schedulerId)).NotNull().NotEmpty();
            Guard.Argument(retryDurableQueueRepository, nameof(retryDurableQueueRepository)).NotNull();
            Guard.Argument(logHandler, nameof(logHandler)).NotNull();
            Guard.Argument(messageHeadersAdapter, nameof(messageHeadersAdapter)).NotNull();
            Guard.Argument(utf8Encoder, nameof(utf8Encoder)).NotNull();
            Guard.Argument(retryDurableMessageProducer, nameof(retryDurableMessageProducer)).NotNull();

            this.retryDurablePollingDefinition = retryDurablePollingDefinition;
            this.trigger = trigger;
            this.jobDetail = JobBuilder
                .Create<RetryDurablePollingJob>()
                .WithIdentity($"pollingJob_{schedulerId}_{retryDurablePollingDefinition.PollingJobType}", "queueTrackerGroup")
                .SetJobData(
                    new JobDataMap
                    {
                        { PollingJobConstants.RetryDurablePollingDefinition, retryDurablePollingDefinition },
                        { PollingJobConstants.SchedulerId, schedulerId },
                        { PollingJobConstants.RetryDurableQueueRepository, retryDurableQueueRepository },
                        { PollingJobConstants.LogHandler, logHandler },
                        { PollingJobConstants.MessageHeadersAdapter, messageHeadersAdapter },
                        { PollingJobConstants.Utf8Encoder, utf8Encoder },
                        { PollingJobConstants.RetryDurableMessageProducer, retryDurableMessageProducer }
                    })
                .Build();
        }

    public IJobDetail JobDetail => this.jobDetail;

    public PollingDefinition PollingDefinition => this.retryDurablePollingDefinition;

    public ITrigger Trigger => this.trigger;
}