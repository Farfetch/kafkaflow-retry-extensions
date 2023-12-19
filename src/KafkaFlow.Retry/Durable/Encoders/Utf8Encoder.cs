﻿using System.Text;

namespace KafkaFlow.Retry.Durable.Encoders;

internal class Utf8Encoder : IUtf8Encoder
{
    public string Decode(byte[] data) => data is null ? null : Encoding.UTF8.GetString(data);

    public byte[] Encode(string data) => Encoding.UTF8.GetBytes(data);
}