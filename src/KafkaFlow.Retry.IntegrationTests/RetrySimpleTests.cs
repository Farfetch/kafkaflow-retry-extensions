using System;
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
public class RetrySimpleTests
{
    private readonly BootstrapperHostFixture bootstrapperHostFixture;
    private readonly Fixture fixture = new Fixture();
    private IServiceProvider provider;

    public RetrySimpleTests(BootstrapperHostFixture bootstrapperHostFixture)
    {
        this.bootstrapperHostFixture = bootstrapperHostFixture;
        InMemoryAuxiliarStorage<RetrySimpleTestMessage>.Clear();
    }

    [Fact]
    public async Task RetrySimpleTest()
    {
        // Arrange
        var producer1 = bootstrapperHostFixture.ServiceProvider.GetRequiredService<IMessageProducer<RetrySimpleProducer>>();
        var messages = fixture.CreateMany<RetrySimpleTestMessage>(10).ToList();

        // Act
        messages.ForEach(m => producer1.Produce(m.Key, m));

        // Assert
        foreach (var message in messages)
        {
            await InMemoryAuxiliarStorage<RetrySimpleTestMessage>.AssertCountMessageAsync(message, 4);
        }
    }
}