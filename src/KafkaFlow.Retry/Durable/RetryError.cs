namespace KafkaFlow.Retry.Durable;

public class RetryError
{
    public RetryError(RetryErrorCode code)
    {
        Code = code;
    }

    public RetryErrorCode Code { get; set; }
}