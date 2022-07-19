using Dawn;
using KafkaFlow.Retry.Durable.Definitions;
using KafkaFlow.Retry.Durable.Encoders;
using KafkaFlow.Retry.Durable.Repository;
using KafkaFlow.Retry.Durable.Repository.Adapters;
using Quartz;

namespace KafkaFlow.Retry.Durable.Polling
{
    internal class JobDetailProvider : IJobDetailProvider
    {
        private readonly ILogHandler logHandler;
        private readonly IMessageAdapter messageAdapter;
        private readonly IMessageHeadersAdapter messageHeadersAdapter;
        private readonly IMessageProducer retryDurableMessageProducer;
        private readonly RetryDurablePollingDefinition retryDurablePollingDefinition;
        private readonly IRetryDurableQueueRepository retryDurableQueueRepository;
        private readonly IUtf8Encoder utf8Encoder;

        public JobDetailProvider(
            IRetryDurableQueueRepository retryDurableQueueRepository,
            ILogHandler logHandler,
            IMessageHeadersAdapter messageHeadersAdapter,
            IMessageAdapter messageAdapter,
            IUtf8Encoder utf8Encoder,
            IMessageProducer retryDurableMessageProducer,
            RetryDurablePollingDefinition retryDurablePollingDefinition)
        {
            Guard.Argument(retryDurableQueueRepository).NotNull();
            Guard.Argument(logHandler).NotNull();
            Guard.Argument(messageHeadersAdapter).NotNull();
            Guard.Argument(messageAdapter).NotNull();
            Guard.Argument(utf8Encoder).NotNull();
            Guard.Argument(retryDurableMessageProducer).NotNull();
            Guard.Argument(retryDurablePollingDefinition).NotNull();

            this.retryDurableQueueRepository = retryDurableQueueRepository;
            this.logHandler = logHandler;
            this.messageHeadersAdapter = messageHeadersAdapter;
            this.messageAdapter = messageAdapter;
            this.utf8Encoder = utf8Encoder;
            this.retryDurableMessageProducer = retryDurableMessageProducer;
            this.retryDurablePollingDefinition = retryDurablePollingDefinition;
        }

        public IJobDetail GetQueuePollingJobDetail()
        {
            JobDataMap dataMap = new JobDataMap();
            dataMap.Add(QueuePollingJobConstants.RetryDurableQueueRepository, this.retryDurableQueueRepository);
            dataMap.Add(QueuePollingJobConstants.RetryDurableMessageProducer, this.retryDurableMessageProducer);
            dataMap.Add(QueuePollingJobConstants.RetryDurablePollingDefinition, this.retryDurablePollingDefinition);
            dataMap.Add(QueuePollingJobConstants.LogHandler, this.logHandler);
            dataMap.Add(QueuePollingJobConstants.MessageHeadersAdapter, this.messageHeadersAdapter);
            dataMap.Add(QueuePollingJobConstants.MessageAdapter, this.messageAdapter);
            dataMap.Add(QueuePollingJobConstants.Utf8Encoder, this.utf8Encoder);

            return JobBuilder
                .Create<QueuePollingJob>()
                .SetJobData(dataMap)
                .Build();
        }
    }
}