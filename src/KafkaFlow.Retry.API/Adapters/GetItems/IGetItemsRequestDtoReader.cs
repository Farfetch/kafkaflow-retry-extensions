using KafkaFlow.Retry.API.Dtos;
using Microsoft.AspNetCore.Http;

namespace KafkaFlow.Retry.API.Adapters.GetItems;

public interface IGetItemsRequestDtoReader
{
    GetItemsRequestDto Read(HttpRequest request);
}