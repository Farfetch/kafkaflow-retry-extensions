namespace KafkaFlow.Retry.IntegrationTests.Core.Handlers
{
    using System.Threading.Tasks;
    using KafkaFlow;
    using KafkaFlow.Retry.IntegrationTests.Core.Exceptions;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;
    using KafkaFlow.TypedHandler;

    internal class RetrySimpleTestMessageHandler : IMessageHandler<RetrySimpleTestMessage>
    {
        public Task Handle(IMessageContext context, RetrySimpleTestMessage message)
        {
            MessageStorage.Add(message);

            throw new RetrySimpleException();
        }
    }
}