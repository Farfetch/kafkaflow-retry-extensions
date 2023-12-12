using System.Threading.Tasks;
using KafkaFlow;
using KafkaFlow.Retry.IntegrationTests.Core.Exceptions;
using KafkaFlow.Retry.IntegrationTests.Core.Messages;
using KafkaFlow.Retry.IntegrationTests.Core.Storages;

namespace KafkaFlow.Retry.IntegrationTests.Core.Handlers;

internal class RetrySimpleTestMessageHandler : IMessageHandler<RetrySimpleTestMessage>
{
    public Task Handle(IMessageContext context, RetrySimpleTestMessage message)
    {
        InMemoryAuxiliarStorage<RetrySimpleTestMessage>.Add(message);

        throw new RetrySimpleTestException();
    }
}