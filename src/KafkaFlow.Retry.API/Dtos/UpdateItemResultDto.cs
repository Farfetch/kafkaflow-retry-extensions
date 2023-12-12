using System;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;

namespace KafkaFlow.Retry.API.Dtos;

public class UpdateItemResultDto
{
    public UpdateItemResultDto(Guid itemId, UpdateItemResultStatus value)
    {
            this.ItemId = itemId;
            this.Result = value.ToString();
        }

    public Guid ItemId { get; set; }

    public string Result { get; set; }
}