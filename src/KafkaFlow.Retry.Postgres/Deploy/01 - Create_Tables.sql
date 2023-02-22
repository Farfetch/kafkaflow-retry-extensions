-- Create Tables and Indexes

CREATE SCHEMA IF NOT EXISTS dbo;

-- CREATE TABLES

CREATE TABLE IF NOT EXISTS dbo.QueueStatus (
    Code smallint PRIMARY KEY NOT NULL,
    Name varchar(255) NOT NULL,
    Description varchar(255) NOT NULL);

CREATE TABLE IF NOT EXISTS dbo.QueueItemStatus (
    Code smallint PRIMARY KEY NOT NULL,
    Name varchar(255) NOT NULL,
    Description varchar(255) NOT NULL);

CREATE TABLE IF NOT EXISTS dbo.QueueItemSeverity (
    Code smallint PRIMARY KEY NOT NULL,
    Name varchar(255) NOT NULL,
    Description varchar(255) NOT NULL);

CREATE TABLE IF NOT EXISTS dbo.RetryQueues (
    Id bigint PRIMARY KEY NOT NULL GENERATED ALWAYS AS IDENTITY,
    IdDomain uuid UNIQUE,
    IdStatus smallint NOT NULL,
    SearchGroupKey varchar(255) NOT NULL,
    QueueGroupKey varchar(255) NOT NULL,
    CreationDate TIMESTAMP NOT NULL,
    LastExecution TIMESTAMP NOT NULL,
    CONSTRAINT FK_RetryQueueStatus_RetryQueues FOREIGN KEY (IdStatus) REFERENCES dbo.QueueStatus(Code)
    );

CREATE TABLE IF NOT EXISTS dbo.RetryQueueItems (
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
    CONSTRAINT FK_RetryQueues_RetryQueueItems_IdRetryQueue FOREIGN KEY (IdRetryQueue) REFERENCES dbo.RetryQueues(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RetryQueues_RetryQueueItems_IdDomainRetryQueue FOREIGN KEY (IdDomainRetryQueue) REFERENCES dbo.RetryQueues(IdDomain),
    CONSTRAINT FK_QueueItemStatus_RetryQueueItems FOREIGN KEY (IdItemStatus) REFERENCES dbo.QueueItemStatus(Code),
    CONSTRAINT FK_QueueItemSeverity_RetryQueueItems FOREIGN KEY (IdSeverityLevel) REFERENCES dbo.QueueItemSeverity(Code)
    );

CREATE TABLE IF NOT EXISTS dbo.ItemMessages (
    IdRetryQueueItem bigint PRIMARY KEY,		
    Key bytea NOT NULL,
    Value bytea NOT NULL,
    TopicName varchar(300) NOT NULL,
    Partition int NOT NULL,
    "Offset" bigint NOT NULL,
    UtcTimeStamp TIMESTAMP NOT NULL,
    CONSTRAINT FK_RetryQueueItems_ItemMessages FOREIGN KEY (IdRetryQueueItem) REFERENCES dbo.RetryQueueItems(Id) ON DELETE CASCADE
    );

CREATE TABLE IF NOT EXISTS dbo.RetryItemMessageHeaders (
    Id bigint PRIMARY KEY NOT NULL GENERATED ALWAYS AS IDENTITY NOT NULL,
    IdItemMessage bigint NOT NULL,
    Key varchar(255) NOT NULL,
    Value bytea NOT NULL,
    CONSTRAINT FK_ItemMessages_RetryItemMessageHeaders FOREIGN KEY (IdItemMessage) REFERENCES dbo.ItemMessages(IdRetryQueueItem) ON DELETE CASCADE
    );

CREATE OR REPLACE FUNCTION dbo.P_LoadItemMessages(retryQueueItemsIds bigint[])
    RETURNS TABLE (
      IdRetryQueueItem bigint,
      Key bytea,
      Value bytea,
      TopicName varchar(300),
      Partition int,
      "Offset" bigint,
      UtcTimeStamp TIMESTAMP
    )
    LANGUAGE plpgsql
    AS $$
    BEGIN
    RETURN QUERY
        SELECT IM.IdRetryQueueItem, IM.Key, IM.Value, IM.TopicName, IM.Partition, IM."Offset", IM.UtcTimeStamp
        FROM dbo.ItemMessages IM
                 INNER JOIN dbo.RetryQueueItems RQI ON RQI.Id = IM.IdRetryQueueItem
                 INNER JOIN UNNEST(retryQueueItemsIds) AS RI(Id) ON IM.IdRetryQueueItem=RI.Id
        ORDER BY RQI.IdRetryQueue, IM.IdRetryQueueItem;
    END $$;

-- CREATE INDEXES

-- Table RetryQueues
CREATE INDEX IF NOT EXISTS IX_RetryQueues_SearchGroupKey ON dbo.RetryQueues (SearchGroupKey);

CREATE UNIQUE INDEX IF NOT EXISTS IX_RetryQueues_QueueGroupKey ON dbo.RetryQueues (QueueGroupKey);

CREATE INDEX IF NOT EXISTS IX_RetryQueues_IdStatus ON dbo.RetryQueues (IdStatus);

CREATE INDEX IF NOT EXISTS IX_RetryQueues_CreationDate ON dbo.RetryQueues (CreationDate);

CREATE INDEX IF NOT EXISTS IX_RetryQueues_LastExecution ON dbo.RetryQueues (LastExecution);

-- Table RetryQueuesItems
CREATE UNIQUE INDEX IF NOT EXISTS IX_RetryQueueItems_IdDomain ON dbo.RetryQueueItems (IdDomain);

CREATE INDEX IF NOT EXISTS IX_RetryQueueItems_Sort ON dbo.RetryQueueItems (Sort);

CREATE INDEX IF NOT EXISTS IX_RetryQueueItems_IdItemStatus ON dbo.RetryQueueItems (IdItemStatus);

CREATE INDEX IF NOT EXISTS IX_RetryQueueItems_IdSeverityLevel ON dbo.RetryQueueItems (IdSeverityLevel);	