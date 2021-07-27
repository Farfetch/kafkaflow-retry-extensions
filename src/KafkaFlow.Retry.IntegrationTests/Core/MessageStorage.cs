namespace KafkaFlow.Retry.IntegrationTests.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;
    using Xunit;

    internal static class MessageStorage
    {
        private const int TimeoutSec = 8;
        private static readonly ConcurrentBag<RetryForeverTestMessage> RetryForeverMessage = new ConcurrentBag<RetryForeverTestMessage>();
        private static readonly ConcurrentBag<RetrySimpleTestMessage> RetrySimpleMessage = new ConcurrentBag<RetrySimpleTestMessage>();

        public static void Add(RetrySimpleTestMessage message)
        {
            RetrySimpleMessage.Add(message);
        }

        public static void Add(RetryForeverTestMessage message)
        {
            RetryForeverMessage.Add(message);
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
        }
    }
}