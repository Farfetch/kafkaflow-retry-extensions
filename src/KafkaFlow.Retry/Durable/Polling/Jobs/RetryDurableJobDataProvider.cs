namespace KafkaFlow.Retry.Durable.Polling.Jobs
{
    using Dawn;
    using KafkaFlow.Retry.Durable.Definitions.Polling;
    using KafkaFlow.Retry.Durable.Encoders;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Repository.Adapters;
    using Quartz;

    internal class RetryDurableJobDataProvider : IJobDataProvider
    {
        private readonly ILogHandler logHandler;
        private readonly IMessageHeadersAdapter messageHeadersAdapter;
        private readonly IMessageProducer retryDurableMessageProducer;
        private readonly RetryDurablePollingDefinition retryDurablePollingDefinition;
        private readonly IRetryDurableQueueRepository retryDurableQueueRepository;
        private readonly string schedulerId;
        private readonly ITrigger trigger;
        private readonly IUtf8Encoder utf8Encoder;

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
            this.schedulerId = schedulerId;
            this.retryDurableQueueRepository = retryDurableQueueRepository;
            this.logHandler = logHandler;
            this.messageHeadersAdapter = messageHeadersAdapter;
            this.utf8Encoder = utf8Encoder;
            this.retryDurableMessageProducer = retryDurableMessageProducer;
        }

        public PollingDefinition PollingDefinition => this.retryDurablePollingDefinition;

        public ITrigger Trigger => this.trigger;

        public IJobDetail GetPollingJobDetail()
            => JobBuilder
                .Create<RetryDurablePollingJob>()
                .WithIdentity($"pollingJob_{this.schedulerId}_{this.retryDurablePollingDefinition.PollingJobType}", "queueTrackerGroup")
                .SetJobData(
                    new JobDataMap
                    {
                        { PollingJobConstants.RetryDurablePollingDefinition, this.retryDurablePollingDefinition },
                        { PollingJobConstants.SchedulerId, this.schedulerId },
                        { PollingJobConstants.RetryDurableQueueRepository, this.retryDurableQueueRepository },
                        { PollingJobConstants.LogHandler, this.logHandler },
                        { PollingJobConstants.MessageHeadersAdapter, this.messageHeadersAdapter },
                        { PollingJobConstants.Utf8Encoder, this.utf8Encoder },
                        { PollingJobConstants.RetryDurableMessageProducer, this.retryDurableMessageProducer }
                    })
                .Build();
    }
}