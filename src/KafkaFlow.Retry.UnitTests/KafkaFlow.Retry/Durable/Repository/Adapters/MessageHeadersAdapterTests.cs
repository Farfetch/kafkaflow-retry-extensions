using System.Collections.Generic;
using KafkaFlow.Retry.Durable.Repository.Adapters;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Repository.Adapters;

public class MessageHeadersAdapterTests
{
    private readonly MessageHeadersAdapter adapter = new MessageHeadersAdapter();

    [Fact]
    public void MessageHeadersAdapter_AdaptMessageHeadersFromRepository_Success()
    {
            // Arrange
            var fromMessageHeaders = new List<MessageHeader>
            {
                new MessageHeader("key", new byte[0])
            };

            // Act
            var result = adapter.AdaptMessageHeadersFromRepository(fromMessageHeaders);

            // Assert
            result.Should().HaveCount(1);
        }

    [Fact]
    public void MessageHeadersAdapter_AdaptMessageHeadersToRepository_Success()
    {
            // Arrange
            var messageHeadersTest = new MessageHeadersTest
            {
                { "key", new byte[0] }
            };

            // Act
            var result = adapter.AdaptMessageHeadersToRepository(messageHeadersTest);

            // Assert
            result.Should().HaveCount(1);
        }

    private class MessageHeadersTest : IMessageHeaders
    {
        private readonly IDictionary<string, byte[]> keyValuePairs = new Dictionary<string, byte[]>();

        public byte[] this[string key] { get => keyValuePairs[key]; set => keyValuePairs[key] = value; }

        public void Add(string key, byte[] value)
        {
                keyValuePairs.Add(key, value);
            }

        public IEnumerator<KeyValuePair<string, byte[]>> GetEnumerator()
        {
                return keyValuePairs.GetEnumerator();
            }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
                return keyValuePairs.GetEnumerator();
            }
    }
}