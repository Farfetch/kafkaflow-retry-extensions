namespace KafkaFlow.Retry.Durable.Polling.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class PollingJobStrategyProvider : IPollingJobStrategyProvider // TODO: why not factory pattern?
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

        public IPollingJobStrategy GetPollingJobStrategy(PollingStrategy strategy)
        {
            return this.pollingJobStrategies.Single(x => Enum.Equals(x.Strategy, strategy)); // TODO: avoid linq here. use and simple array or if/else because we have just two.
        }
    }
}