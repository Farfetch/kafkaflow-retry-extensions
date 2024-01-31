using Dawn;
using KafkaFlow.Retry.API.Dtos;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;

namespace KafkaFlow.Retry.API.Adapters.UpdateItems;

internal class UpdateItemsResponseDtoAdapter : IUpdateItemsResponseDtoAdapter
{
    public UpdateItemsResponseDto Adapt(UpdateItemsResult updateItemsResult)
    {
        Guard.Argument(updateItemsResult, nameof(updateItemsResult)).NotNull();

        var resultDto = new UpdateItemsResponseDto();

        foreach (var result in updateItemsResult.Results)
        {
            resultDto.UpdateItemsResults.Add(new UpdateItemResultDto(result.Id, result.Status));
        }

        return resultDto;
    }
}