namespace KafkaFlow.Retry.Postgres.Readers
{
    using System;
    using System.Collections.Generic;
    using Dawn;
    
    internal class DboCollectionNavigator<TDbo, TDomain> where TDbo : class
    {
        private readonly IDboDomainAdapter<TDbo, TDomain> dboDomainAdapter;
        private readonly IList<TDbo> dbos;
        private int currentIndex;

        public DboCollectionNavigator(IList<TDbo> dbos, IDboDomainAdapter<TDbo, TDomain> dboDomainAdapter)
        {
            Guard.Argument(dbos, nameof(dbos)).NotNull();
            Guard.Argument(dboDomainAdapter, nameof(dboDomainAdapter)).NotNull();

            this.dboDomainAdapter = dboDomainAdapter;
            this.dbos = dbos;
        }

        public void Navigate(Action<TDomain> action, Predicate<TDbo> navigatingCondition)
        {
            Guard.Argument(action).NotNull();
            Guard.Argument(navigatingCondition).NotNull();

            this.Navigate((domain, _) => action(domain), navigatingCondition);
        }

        public void Navigate(Action<TDomain, TDbo> action, Predicate<TDbo> navigatingCondition)
        {
            Guard.Argument(action).NotNull();
            Guard.Argument(navigatingCondition).NotNull();

            while (this.currentIndex < this.dbos.Count)
            {
                var currentDbo = this.dbos[this.currentIndex];

                if (!navigatingCondition(currentDbo))
                {
                    return;
                }

                action(this.dboDomainAdapter.Adapt(currentDbo), currentDbo);

                this.currentIndex++;
            }
        }
    }
}
