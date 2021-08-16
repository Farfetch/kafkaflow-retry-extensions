namespace KafkaFlow.Retry.IntegrationTests.Core.Handlers
{
    using System.Threading.Tasks;
    using KafkaFlow;
    using KafkaFlow.Retry.IntegrationTests.Core.Exceptions;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages;
    using KafkaFlow.TypedHandler;

    internal class RetryDurableTestMessageHandler : IMessageHandler<RetryDurableTestMessage>
    {
        private readonly ILogHandler logHandler;

        public RetryDurableTestMessageHandler(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }

        public Task Handle(IMessageContext context, RetryDurableTestMessage message)
        {
            this.logHandler.Info($"{message.Key}#{message.Value}", context.ConsumerContext.Offset);

            MessageStorage.Add(message);

            if (MessageStorage.ThrowException)
            {
                throw new RetryDurableTestException();
            }

            return Task.CompletedTask;
        }
    }
}