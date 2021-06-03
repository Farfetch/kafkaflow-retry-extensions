namespace KafkaFlow.Retry.API.Dtos
{
    using System.Collections.Generic;

    public class UpdateItemsResponseDto
    {
        public UpdateItemsResponseDto()
        {
            this.UpdateItemsResults = new List<UpdateItemResultDto>();
        }

        public IList<UpdateItemResultDto> UpdateItemsResults { get; set; }
    }
}
