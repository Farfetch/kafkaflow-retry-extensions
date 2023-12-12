namespace KafkaFlow.Retry.Durable;

public class RetryError
{
    public RetryError(RetryErrorCode code)
    {
        this.Code = code;
    }

    public RetryErrorCode Code { get; set; }
}