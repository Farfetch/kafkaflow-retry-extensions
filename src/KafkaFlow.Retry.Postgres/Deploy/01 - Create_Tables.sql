-- Create Tables and Indexes
-- CREATE TABLES

CREATE TABLE IF NOT EXISTS queue_status (
    Code smallint PRIMARY KEY NOT NULL,
    Name varchar(255) NOT NULL,
    Description varchar(255) NOT NULL);

CREATE TABLE IF NOT EXISTS queue_item_status (
    Code smallint PRIMARY KEY NOT NULL,
    Name varchar(255) NOT NULL,
    Description varchar(255) NOT NULL);

CREATE TABLE IF NOT EXISTS queue_item_severity (
    Code smallint PRIMARY KEY NOT NULL,
    Name varchar(255) NOT NULL,
    Description varchar(255) NOT NULL);

CREATE TABLE IF NOT EXISTS retry_queues (
    Id bigint PRIMARY KEY NOT NULL GENERATED ALWAYS AS IDENTITY,
    IdDomain uuid UNIQUE,
    IdStatus smallint NOT NULL,
    SearchGroupKey varchar(255) NOT NULL,
    QueueGroupKey varchar(255) NOT NULL,
    CreationDate TIMESTAMP NOT NULL,
    LastExecution TIMESTAMP NOT NULL,
    CONSTRAINT FK_RetryQueueStatus_RetryQueues FOREIGN KEY (IdStatus) REFERENCES queue_status(Code)
    );

CREATE TABLE IF NOT EXISTS retry_queue_items (
    Id bigint PRIMARY KEY NOT NULL GENERATED ALWAYS AS IDENTITY,
    IdDomain uuid NOT NULL,
    IdRetryQueue bigint NOT NULL, 
    IdDomainRetryQueue uuid NOT NULL,
    IdItemStatus smallint NOT NULL,
    IdSeverityLevel smallint NOT NULL,
    AttemptsCount int NOT NULL,
    Sort int NOT NULL,
    CreationDate TIMESTAMP NOT NULL,
    LastExecution TIMESTAMP,
    ModifiedStatusDate TIMESTAMP,
    Description varchar NULL,
    CONSTRAINT FK_RetryQueues_RetryQueueItems_IdRetryQueue FOREIGN KEY (IdRetryQueue) REFERENCES retry_queues(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RetryQueues_RetryQueueItems_IdDomainRetryQueue FOREIGN KEY (IdDomainRetryQueue) REFERENCES retry_queues(IdDomain),
    CONSTRAINT FK_QueueItemStatus_RetryQueueItems FOREIGN KEY (IdItemStatus) REFERENCES queue_item_status(Code),
    CONSTRAINT FK_QueueItemSeverity_RetryQueueItems FOREIGN KEY (IdSeverityLevel) REFERENCES queue_item_severity(Code)
    );

CREATE TABLE IF NOT EXISTS item_messages (
    IdRetryQueueItem bigint PRIMARY KEY,		
    Key bytea NOT NULL,
    Value bytea NOT NULL,
    TopicName varchar(300) NOT NULL,
    Partition int NOT NULL,
    "offset" bigint NOT NULL,
    UtcTimeStamp TIMESTAMP NOT NULL,
    CONSTRAINT FK_RetryQueueItems_ItemMessages FOREIGN KEY (IdRetryQueueItem) REFERENCES retry_queue_items(Id) ON DELETE CASCADE
    );

CREATE TABLE IF NOT EXISTS retry_item_message_headers (
    Id bigint PRIMARY KEY NOT NULL GENERATED ALWAYS AS IDENTITY NOT NULL,
    IdItemMessage bigint NOT NULL,
    Key varchar(255) NOT NULL,
    Value bytea NOT NULL,
    CONSTRAINT FK_ItemMessages_RetryItemMessageHeaders FOREIGN KEY (IdItemMessage) REFERENCES item_messages(IdRetryQueueItem) ON DELETE CASCADE
    );

CREATE OR REPLACE FUNCTION f_load_item_messages(retryQueueItemsIds bigint[])
    RETURNS TABLE (
      IdRetryQueueItem bigint,
      Key bytea,
      Value bytea,
      TopicName varchar(300),
      Partition int,
      "offset" bigint,
      UtcTimeStamp TIMESTAMP
    )
    LANGUAGE plpgsql
    AS $$
    BEGIN
    RETURN QUERY
        SELECT IM.IdRetryQueueItem, IM.Key, IM.Value, IM.TopicName, IM.Partition, IM."offset", IM.UtcTimeStamp
        FROM item_messages IM
                 INNER JOIN retry_queue_items RQI ON RQI.Id = IM.IdRetryQueueItem
                 INNER JOIN UNNEST(retryQueueItemsIds) AS RI(Id) ON IM.IdRetryQueueItem=RI.Id
        ORDER BY RQI.IdRetryQueue, IM.IdRetryQueueItem;
    END $$;

-- CREATE INDEXES

-- Table retry_queues
CREATE INDEX IF NOT EXISTS ix_retry_queues_SearchGroupKey ON retry_queues (SearchGroupKey);

CREATE UNIQUE INDEX IF NOT EXISTS ix_retry_queues_QueueGroupKey ON retry_queues (QueueGroupKey);

CREATE INDEX IF NOT EXISTS ix_retry_queues_IdStatus ON retry_queues (IdStatus);

CREATE INDEX IF NOT EXISTS ix_retry_queues_CreationDate ON retry_queues (CreationDate);

CREATE INDEX IF NOT EXISTS ix_retry_queues_LastExecution ON retry_queues (LastExecution);

-- Table retry_queue_items
CREATE UNIQUE INDEX IF NOT EXISTS ix_retry_queue_items_IdDomain ON retry_queue_items (IdDomain);

CREATE INDEX IF NOT EXISTS ix_retry_queue_items_Sort ON retry_queue_items (Sort);

CREATE INDEX IF NOT EXISTS ix_retry_queue_items_IdItemStatus ON retry_queue_items (IdItemStatus);

CREATE INDEX IF NOT EXISTS ix_retry_queue_items_IdSeverityLevel ON retry_queue_items (IdSeverityLevel);	