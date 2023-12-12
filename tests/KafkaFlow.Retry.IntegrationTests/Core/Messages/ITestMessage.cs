namespace KafkaFlow.Retry.IntegrationTests.Core.Messages;

internal interface ITestMessage
{
    string Key { get; set; }
    string Value { get; set; }
}