namespace KafkaFlow.Retry.Durable.Encoders;

internal interface IUtf8Encoder
{
    string Decode(byte[] data);

    byte[] Encode(string data);
}