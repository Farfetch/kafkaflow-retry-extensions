namespace KafkaFlow.Retry.Durable.Polling.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class PollingJobStrategyProvider : IPollingJobStrategyProvider
    {
        private readonly IEnumerable<IPollingJobStrategy> pollingJobStrategies;

        public PollingJobStrategyProvider()
        {
            this.pollingJobStrategies = new List<IPollingJobStrategy>
            {
                new PollingJobStrategyEarliest(),
                new PollingJobStrategyLatest()
            };
        }

        public IPollingJobStrategy GetPollingJobStrategy(PollingJobStrategyType pollingJobStrategyType)
        {
            return this.pollingJobStrategies.Single(x => Enum.Equals(x.PollingJobStrategyType, pollingJobStrategyType));
        }
    }
}