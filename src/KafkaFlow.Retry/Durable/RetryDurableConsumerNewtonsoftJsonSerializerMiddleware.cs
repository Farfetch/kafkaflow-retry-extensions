using System;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.Durable.Serializers;

namespace KafkaFlow.Retry.Durable;

internal class RetryDurableConsumerNewtonsoftJsonSerializerMiddleware : IMessageMiddleware
{
    private readonly INewtonsoftJsonSerializer _newtonsoftJsonSerializer;
    private readonly Type _type;

    public RetryDurableConsumerNewtonsoftJsonSerializerMiddleware(INewtonsoftJsonSerializer newtonsoftJsonSerializer, Type type)
    {
            Guard.Argument(newtonsoftJsonSerializer).NotNull();
            Guard.Argument(type).NotNull();

            _newtonsoftJsonSerializer = newtonsoftJsonSerializer;
            _type = type;
        }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
            await next(context.SetMessage(context.Message.Key, _newtonsoftJsonSerializer.DeserializeObject((string)context.Message.Value, _type))).ConfigureAwait(false);
        }
}