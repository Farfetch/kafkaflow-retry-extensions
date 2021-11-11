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
        private static readonly ConcurrentBag<T> Message = new ConcurrentBag<T>();

        public static bool ThrowException { get; set; }

        public static void Add(T message)
        {
            Message.Add(message);
        }

        public static async Task AssertCountMessageAsync(T message, int count)
        {
            var start = DateTime.Now;

            while (Message.Count(x => x.Key == message.Key && x.Value == message.Value) != count)
            {
                if (DateTime.Now.Subtract(start).TotalSeconds > TimeoutSec && !Debugger.IsAttached)
                {
                    Assert.True(false, "Message not received.");
                    return;
                }

                await Task.Delay(100).ConfigureAwait(false);
            }
        }

        public static void Clear()
        {
            Message.Clear();
        }
    }
}