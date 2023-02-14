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
        private readonly IUtf8Encoder utf8Encoder;

        public RetryDurableJobDataProvider(
            IRetryDurableQueueRepository retryDurableQueueRepository,
            ILogHandler logHandler,
            IMessageHeadersAdapter messageHeadersAdapter,
            IMessageAdapter messageAdapter,
            IUtf8Encoder utf8Encoder,
            IMessageProducer retryDurableMessageProducer,
            RetryDurablePollingDefinition retryDurablePollingDefinition,
            string schedulerId)
        {
            Guard.Argument(retryDurableQueueRepository).NotNull();
            Guard.Argument(logHandler).NotNull();
            Guard.Argument(messageHeadersAdapter).NotNull();
            Guard.Argument(messageAdapter).NotNull();
            Guard.Argument(utf8Encoder).NotNull();
            Guard.Argument(retryDurableMessageProducer).NotNull();
            Guard.Argument(retryDurablePollingDefinition).NotNull();
            Guard.Argument(schedulerId, nameof(schedulerId)).NotNull().NotEmpty();

            this.retryDurableQueueRepository = retryDurableQueueRepository;
            this.logHandler = logHandler;
            this.messageHeadersAdapter = messageHeadersAdapter;
            this.messageAdapter = messageAdapter;
            this.utf8Encoder = utf8Encoder;
            this.retryDurableMessageProducer = retryDurableMessageProducer;
            this.retryDurablePollingDefinition = retryDurablePollingDefinition;
            this.schedulerId = schedulerId;
        }

        public PollingDefinition PollingDefinition => this.retryDurablePollingDefinition;

        public IJobDetail GetPollingJobDetail()
        {
            var dataMap = new JobDataMap();

            dataMap.Add(PollingJobConstants.SchedulerId, this.schedulerId);
            dataMap.Add(PollingJobConstants.RetryDurableQueueRepository, this.retryDurableQueueRepository);
            dataMap.Add(PollingJobConstants.RetryDurableMessageProducer, this.retryDurableMessageProducer);
            dataMap.Add(PollingJobConstants.RetryDurablePollingDefinition, this.retryDurablePollingDefinition);
            dataMap.Add(PollingJobConstants.LogHandler, this.logHandler);
            dataMap.Add(PollingJobConstants.MessageHeadersAdapter, this.messageHeadersAdapter);
            dataMap.Add(PollingJobConstants.MessageAdapter, this.messageAdapter);
            dataMap.Add(PollingJobConstants.Utf8Encoder, this.utf8Encoder);

            return JobBuilder
                .Create<RetryDurablePollingJob>()
                .SetJobData(dataMap)
                .Build();
        }
    }
}