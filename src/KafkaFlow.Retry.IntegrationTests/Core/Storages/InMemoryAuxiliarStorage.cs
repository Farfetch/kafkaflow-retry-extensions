namespace KafkaFlow.Retry.IntegrationTests.Core.Storages
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;
    using Xunit;

    internal static class InMemoryAuxiliarStorage<T> where T : ITestMessage
    {
        private const int TimeoutSec = 60;
        private static readonly ConcurrentBag<T> Messages = new ConcurrentBag<T>();

        public static bool ThrowException { get; set; }

        public static void Add(T message)
        {
            Messages.Add(message);
        }

        public static async Task AssertCountMessageAsync(T message, int count)
        {
            var start = DateTime.Now;

            while (Messages.Count(x => x.Key == message.Key && x.Value == message.Value) != count)
            {
                if (DateTime.Now.Subtract(start).TotalSeconds > TimeoutSec && !Debugger.IsAttached)
                {
                    Assert.True(false, $"Message not received - {message.Key}:{message.Value}.");
                    return;
                }

                await Task.Delay(100).ConfigureAwait(false);
            }
        }

        public static async Task AssertEmptyPartitionKeyCountMessageAsync(T message, int count, int timoutSeconds = TimeoutSec)
        {
            var start = DateTime.Now;
            int numberOfMessages = 0;
            do
            {
                numberOfMessages = Messages.Count(x => x.Value == message.Value);

                if (DateTime.Now.Subtract(start).TotalSeconds > timoutSeconds && !Debugger.IsAttached)
                {
                    Assert.True(false, $"Message {message.Key}:{message.Value} not received. Expected {count}, messages received {numberOfMessages}");
                    return;
                }

                await Task.Delay(1000).ConfigureAwait(false);
            } while (numberOfMessages != count);
        }

        public static void Clear()
        {
            Messages.Clear();
        }
    }
}