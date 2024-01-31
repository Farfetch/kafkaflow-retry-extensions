using System.Collections;
using System.Collections.Generic;
using KafkaFlow.Retry.Durable.Repository.Adapters;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Repository.Adapters;

public class MessageHeadersAdapterTests
{
    private readonly MessageHeadersAdapter _adapter = new();

    [Fact]
    public void MessageHeadersAdapter_AdaptMessageHeadersFromRepository_Success()
    {
        // Arrange
        var fromMessageHeaders = new List<MessageHeader>
        {
            new("key", new byte[0])
        };

        // Act
        var result = _adapter.AdaptMessageHeadersFromRepository(fromMessageHeaders);

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
        var result = _adapter.AdaptMessageHeadersToRepository(messageHeadersTest);

        // Assert
        result.Should().HaveCount(1);
    }

    private class MessageHeadersTest : IMessageHeaders
    {
        private readonly IDictionary<string, byte[]> _keyValuePairs = new Dictionary<string, byte[]>();

        public byte[] this[string key]
        {
            get => _keyValuePairs[key];
            set => _keyValuePairs[key] = value;
        }

        public void Add(string key, byte[] value)
        {
            _keyValuePairs.Add(key, value);
        }

        public IEnumerator<KeyValuePair<string, byte[]>> GetEnumerator()
        {
            return _keyValuePairs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _keyValuePairs.GetEnumerator();
        }
    }
}