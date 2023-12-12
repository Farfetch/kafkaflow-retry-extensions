using System;
using System.Collections.Generic;
using Dawn;

namespace KafkaFlow.Retry.Postgres.Readers;

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

            Navigate((domain, _) => action(domain), navigatingCondition);
        }

    public void Navigate(Action<TDomain, TDbo> action, Predicate<TDbo> navigatingCondition)
    {
            Guard.Argument(action).NotNull();
            Guard.Argument(navigatingCondition).NotNull();

            while (currentIndex < dbos.Count)
            {
                var currentDbo = dbos[currentIndex];

                if (!navigatingCondition(currentDbo))
                {
                    return;
                }

                action(dboDomainAdapter.Adapt(currentDbo), currentDbo);

                currentIndex++;
            }
        }
}