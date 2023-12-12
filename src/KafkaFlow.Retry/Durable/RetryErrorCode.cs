namespace KafkaFlow.Retry.Durable;

public enum RetryErrorCode
{
    Unknown = 0,

    /*
     * AABB
     * AA -> Module
     * BB -> Error number
     */

    // MAIN CONSUMER MODULE NUMBER: 00
    ConsumerKafkaException = 0001,

    ConsumerUnrecoverableException = 0002,
    ConsumerBlockedException = 0003,
    ConsumerIgnoredException = 0004,
    ConsumerHandledException = 0005,

    // POLLING MODULE NUMBER: 01
    PollingUnknownException = 0101,

    PollingProducerException = 0102,

    // DATA PROVIDER MODULE NUMBER: 02
    DataProviderUnrecoverableException = 0201,

    DataProviderSaveToQueue = 0202,
    DataProviderAddIfQueueExists = 0203,
    DataProviderCheckQueuePendingItems = 0204,
    DataProviderGetRetryQueues = 0205,
    DataProviderUpdateItem = 0206,
}