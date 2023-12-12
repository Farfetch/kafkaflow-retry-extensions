using Dawn;

namespace KafkaFlow.Retry.Postgres.Model.Schema;

public class Script
{
    public Script(string value)
    {
        Guard.Argument(value, nameof(value)).NotNull();

        Value = value;
    }

    public string Value { get; set; }
}