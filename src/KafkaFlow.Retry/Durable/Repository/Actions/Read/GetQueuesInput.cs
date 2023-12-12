using System.Collections.Generic;
using System.Linq;
using Dawn;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.Durable.Repository.Actions.Read;

public class GetQueuesInput
{
    public GetQueuesInput(RetryQueueStatus status,
        IEnumerable<RetryQueueItemStatus> itemsStatuses,
        GetQueuesSortOption sortOption,
        int topQueues,
        StuckStatusFilter stuckStatusFilter = null)
    {
        Guard.Argument(status, nameof(status)).NotDefault();
        Guard.Argument(itemsStatuses, nameof(itemsStatuses))
            .NotNull()
            .Require(statuses => !statuses.Contains(RetryQueueItemStatus.None));
        Guard.Argument(sortOption, nameof(sortOption)).NotDefault();
        Guard.Argument(topQueues, nameof(topQueues)).NotZero().NotNegative();

        if (stuckStatusFilter is object)
        {
            Guard.Argument(itemsStatuses, nameof(itemsStatuses))
                .DoesNotContain(stuckStatusFilter.ItemStatus,
                    (statuses, stuckStatus) =>
                        "The status list can't contain the status that can be considered as stuck.");
        }

        Status = status;
        ItemsStatuses = itemsStatuses;
        SortOption = sortOption;
        TopQueues = topQueues;
        StuckStatusFilter = stuckStatusFilter;
    }

    public IEnumerable<RetryQueueItemStatus> ItemsStatuses { get; }

    public string SearchGroupKey { get; set; }

    public IEnumerable<SeverityLevel> SeverityLevels { get; set; }

    public GetQueuesSortOption SortOption { get; }

    public RetryQueueStatus Status { get; }

    public StuckStatusFilter StuckStatusFilter { get; }

    public int? TopItemsByQueue { get; set; }

    public int TopQueues { get; }
}