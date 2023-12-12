using System.Collections.Generic;

namespace KafkaFlow.Retry.API.Dtos;

public class UpdateQueuesResponseDto
{
    public UpdateQueuesResponseDto()
    {
        this.UpdateQueuesResults = new List<UpdateQueueResultDto>();
    }

    public IList<UpdateQueueResultDto> UpdateQueuesResults { get; set; }
}