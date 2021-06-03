namespace KafkaFlow.Retry.API.Dtos
{
    using System.Collections.Generic;

    public class UpdateQueuesResponseDto
    {
        public UpdateQueuesResponseDto()
        {
            this.UpdateQueuesResults = new List<UpdateQueueResultDto>();
        }

        public IList<UpdateQueueResultDto> UpdateQueuesResults { get; set; }
    }
}
