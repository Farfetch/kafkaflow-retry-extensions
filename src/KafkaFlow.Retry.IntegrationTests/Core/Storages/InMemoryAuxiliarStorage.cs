namespace KafkaFlow.Retry.IntegrationTests.Core.Storages
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;
    using Xunit;

    internal static class InMemoryAuxiliarStorage
    {
        private const int TimeoutSec = 60;
        private static readonly ConcurrentBag<RetryDurableTestMessage> RetryDurableMessage = new ConcurrentBag<RetryDurableTestMessage>();
        private static readonly ConcurrentBag<RetryForeverTestMessage> RetryForeverMessage = new ConcurrentBag<RetryForeverTestMessage>();
        private static readonly ConcurrentBag<RetrySimpleTestMessage> RetrySimpleMessage = new ConcurrentBag<RetrySimpleTestMessage>();

        public static bool ThrowException { get; set; }

        public static void Add(RetrySimpleTestMessage message)
        {
            RetrySimpleMessage.Add(message);
        }

        public static void Add(RetryForeverTestMessage message)
        {
            RetryForeverMessage.Add(message);
        }

        public static void Add(RetryDurableTestMessage message)
        {
            RetryDurableMessage.Add(message);
        }

        public static async Task AssertCountRetryDurableMessageAsync(RetryDurableTestMessage message, int count)
        {
            var start = DateTime.Now;

            while (RetryDurableMessage.Count(x => x.Key == message.Key && x.Value == message.Value) != count)
            {
                if (DateTime.Now.Subtract(start).Seconds > TimeoutSec)
                {
                    Assert.True(false, "Message not received.");
                    return;
                }

                await Task.Delay(100).ConfigureAwait(false);
            }
        }

        public static async Task AssertCountRetryForeverMessageAsync(RetryForeverTestMessage message, int count)
        {
            var start = DateTime.Now;

            while (RetryForeverMessage.Count(x => x.Key == message.Key && x.Value == message.Value) != count)
            {
                if (DateTime.Now.Subtract(start).Seconds > TimeoutSec)
                {
                    Assert.True(false, "Message not received.");
                    return;
                }

                await Task.Delay(100).ConfigureAwait(false);
            }
        }

        public static async Task AssertCountRetrySimpleMessageAsync(RetrySimpleTestMessage message, int count)
        {
            var start = DateTime.Now;

            while (RetrySimpleMessage.Count(x => x.Key == message.Key && x.Value == message.Value) != count)
            {
                if (DateTime.Now.Subtract(start).Seconds > TimeoutSec)
                {
                    Assert.True(false, "Message not received.");
                    return;
                }

                await Task.Delay(100).ConfigureAwait(false);
            }
        }

        public static void Clear()
        {
            RetrySimpleMessage.Clear();
            RetryForeverMessage.Clear();
            RetryDurableMessage.Clear();
        }
    }
}