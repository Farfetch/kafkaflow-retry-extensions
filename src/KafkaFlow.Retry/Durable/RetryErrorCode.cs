namespace KafkaFlow.Retry.Durable
{
    public enum RetryErrorCode
    {
        Unknown = 0,

        /*
         * AABB
         * AA -> Module
         * BB -> Error number
         */

        // MAIN CONSUMER MODULE NUMBER: 00
        Consumer_KafkaException = 0001,

        Consumer_UnrecoverableException = 0002,
        Consumer_BlockedException = 0003,
        Consumer_IgnoredException = 0004,
        Consumer_HandledException = 0005,

        // POLLING MODULE NUMBER: 01
        Polling_UnknownException = 0101,

        Polling_ProducerException = 0102,

        // DATA PROVIDER MODULE NUMBER: 02
        DataProvider_UnrecoverableException = 0201,

        DataProvider_SaveToQueue = 0202,
        DataProvider_AddIfQueueExists = 0203,
        DataProvider_CheckQueuePendingItems = 0204,
        DataProvider_GetRetryQueues = 0205,
        DataProvider_UpdateItem = 0206,
    }
}