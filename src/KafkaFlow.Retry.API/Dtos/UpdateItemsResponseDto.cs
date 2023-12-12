using System.Collections.Generic;

namespace KafkaFlow.Retry.API.Dtos;

public class UpdateItemsResponseDto
{
    public UpdateItemsResponseDto()
    {
            UpdateItemsResults = new List<UpdateItemResultDto>();
        }

    public IList<UpdateItemResultDto> UpdateItemsResults { get; set; }
}