using System;
using System.Diagnostics.CodeAnalysis;
using Dawn;

namespace KafkaFlow.Retry.Durable.Repository.Actions.Update;

[ExcludeFromCodeCoverage]
public class UpdateItemResult
{
    public UpdateItemResult(Guid id, UpdateItemResultStatus status)
    {
            Guard.Argument(id, nameof(id)).NotDefault();
            Guard.Argument(status, nameof(status)).NotDefault();

            this.Id = id;
            this.Status = status;
        }

    public Guid Id { get; }

    public UpdateItemResultStatus Status { get; }
}