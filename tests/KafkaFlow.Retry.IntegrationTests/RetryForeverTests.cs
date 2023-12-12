using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;
using KafkaFlow.Retry.IntegrationTests.Core.Messages;
using KafkaFlow.Retry.IntegrationTests.Core.Producers;
using KafkaFlow.Retry.IntegrationTests.Core.Storages;
using Microsoft.Extensions.DependencyInjection;

namespace KafkaFlow.Retry.IntegrationTests;

[Collection("BootstrapperHostCollection")]
public class RetryForeverTests
{
    private readonly BootstrapperHostFixture _bootstrapperHostFixture;
    private readonly Fixture _fixture = new Fixture();

    public RetryForeverTests(BootstrapperHostFixture bootstrapperHostFixture)
    {
        _bootstrapperHostFixture = bootstrapperHostFixture;
        InMemoryAuxiliarStorage<RetryForeverTestMessage>.Clear();
        InMemoryAuxiliarStorage<RetryForeverTestMessage>.ThrowException = true;
    }

    [Fact]
    public async Task RetryForeverTest()
    {
        // Arrange
        var producer1 = _bootstrapperHostFixture.ServiceProvider.GetRequiredService<IMessageProducer<RetryForeverProducer>>();
        var messages = _fixture.CreateMany<RetryForeverTestMessage>(1).ToList();

        // Act
        messages.ForEach(m => producer1.Produce(m.Key, m));

        // Assert
        foreach (var message in messages)
        {
            await InMemoryAuxiliarStorage<RetryForeverTestMessage>.AssertCountMessageAsync(message, 20);
        }

        // To avoid a message not committed on the tests topic
        InMemoryAuxiliarStorage<RetryForeverTestMessage>.Clear();
        InMemoryAuxiliarStorage<RetryForeverTestMessage>.ThrowException = false;

        foreach (var message in messages)
        {
            await InMemoryAuxiliarStorage<RetryForeverTestMessage>.AssertCountMessageAsync(message, 1);
        }
    }
}