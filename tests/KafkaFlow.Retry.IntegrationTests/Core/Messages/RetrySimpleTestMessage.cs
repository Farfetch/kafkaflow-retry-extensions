namespace KafkaFlow.Retry.IntegrationTests.Core.Messages;

internal class RetrySimpleTestMessage : ITestMessage
{
    public string Key { get; set; }
    public string Value { get; set; }
}