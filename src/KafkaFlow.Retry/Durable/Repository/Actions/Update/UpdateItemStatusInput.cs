using System;
using System.Diagnostics.CodeAnalysis;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.Durable.Repository.Actions.Update;

[ExcludeFromCodeCoverage]
public class UpdateItemStatusInput : UpdateItemInput
{
    public UpdateItemStatusInput(Guid itemId, RetryQueueItemStatus status)
        : base(itemId, status)
    {
        }
}