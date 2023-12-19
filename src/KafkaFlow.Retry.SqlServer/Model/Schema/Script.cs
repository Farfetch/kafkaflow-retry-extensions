﻿using Dawn;

namespace KafkaFlow.Retry.SqlServer.Model.Schema;

public class Script
{
    public Script(string value)
    {
            Guard.Argument(value, nameof(value)).NotNull();

            this.Value = value;
        }

    public string Value { get; set; }
}