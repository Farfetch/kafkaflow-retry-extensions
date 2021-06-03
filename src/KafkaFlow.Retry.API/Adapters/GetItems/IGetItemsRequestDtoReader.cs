namespace KafkaFlow.Retry.API.Adapters.GetItems
{
    using KafkaFlow.Retry.API.Dtos;
    using Microsoft.AspNetCore.Http;

    public interface IGetItemsRequestDtoReader
    {
        GetItemsRequestDto Read(HttpRequest request);
    }
}
