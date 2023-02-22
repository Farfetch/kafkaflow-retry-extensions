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
        private readonly IMessageAdapter messageAdapter;
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
            IMessageAdapter messageAdapter,
            IUtf8Encoder utf8Encoder,
            IMessageProducer retryDurableMessageProducer)
        {
            Guard.Argument(retryDurablePollingDefinition, nameof(retryDurablePollingDefinition)).NotNull();
            Guard.Argument(trigger, nameof(trigger)).NotNull();
            Guard.Argument(schedulerId, nameof(schedulerId)).NotNull().NotEmpty();
            Guard.Argument(retryDurableQueueRepository, nameof(retryDurableQueueRepository)).NotNull();
            Guard.Argument(logHandler, nameof(logHandler)).NotNull();
            Guard.Argument(messageHeadersAdapter, nameof(messageHeadersAdapter)).NotNull();
            Guard.Argument(messageAdapter, nameof(messageAdapter)).NotNull();
            Guard.Argument(utf8Encoder, nameof(utf8Encoder)).NotNull();
            Guard.Argument(retryDurableMessageProducer, nameof(retryDurableMessageProducer)).NotNull();

            this.retryDurablePollingDefinition = retryDurablePollingDefinition;
            this.trigger = trigger;
            this.schedulerId = schedulerId;
            this.retryDurableQueueRepository = retryDurableQueueRepository;
            this.logHandler = logHandler;
            this.messageHeadersAdapter = messageHeadersAdapter;
            this.messageAdapter = messageAdapter;
            this.utf8Encoder = utf8Encoder;
            this.retryDurableMessageProducer = retryDurableMessageProducer;
        }

        public PollingDefinition PollingDefinition => this.retryDurablePollingDefinition;

        public ITrigger Trigger => this.trigger;

        public IJobDetail GetPollingJobDetail()
        {
            var dataMap = new JobDataMap();

            dataMap.Add(PollingJobConstants.RetryDurablePollingDefinition, this.retryDurablePollingDefinition);
            dataMap.Add(PollingJobConstants.SchedulerId, this.schedulerId);
            dataMap.Add(PollingJobConstants.RetryDurableQueueRepository, this.retryDurableQueueRepository);
            dataMap.Add(PollingJobConstants.LogHandler, this.logHandler);
            dataMap.Add(PollingJobConstants.MessageHeadersAdapter, this.messageHeadersAdapter);
            dataMap.Add(PollingJobConstants.MessageAdapter, this.messageAdapter);
            dataMap.Add(PollingJobConstants.Utf8Encoder, this.utf8Encoder);
            dataMap.Add(PollingJobConstants.RetryDurableMessageProducer, this.retryDurableMessageProducer);

            return JobBuilder
                .Create<RetryDurablePollingJob>()
                .SetJobData(dataMap)
                .Build();
        }
    }
}