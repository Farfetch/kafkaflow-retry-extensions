-- Create Tables and Indexes

USE @dbname

-- CREATE TABLES

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'QueueStatus'))
BEGIN
    CREATE TABLE [dbo].[QueueStatus] (
	[Code] [tinyint] PRIMARY KEY NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[Description] [varchar](255) NOT NULL)

	PRINT 'Created table [QueueStatus]'
END
GO

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'QueueItemStatus'))
BEGIN

	CREATE TABLE [dbo].[QueueItemStatus] (
		[Code] [tinyint] PRIMARY KEY NOT NULL,
		[Name] [varchar](255) NOT NULL,
		[Description] [varchar](255) NOT NULL)

	PRINT 'Created table [QueueItemStatus]'
END
GO

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'QueueItemSeverity'))
BEGIN

	CREATE TABLE [dbo].[QueueItemSeverity] (
		[Code] [tinyint] PRIMARY KEY NOT NULL,
		[Name] [varchar](255) NOT NULL,
		[Description] [varchar](255) NOT NULL)

	PRINT 'Created table [QueueItemSeverity]'
END
GO

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'RetryQueues'))
BEGIN

	CREATE TABLE [dbo].[RetryQueues] (
		[Id] [bigint] PRIMARY KEY IDENTITY(1,1),
		[IdDomain] [uniqueidentifier] UNIQUE,
		[IdStatus] [tinyint] NOT NULL,
		[SearchGroupKey] [nvarchar](255) NOT NULL,
		[QueueGroupKey] [nvarchar](255) NOT NULL,
		[CreationDate] [datetime2](7) NOT NULL,
		[LastExecution] [datetime2](7) NOT NULL,
		CONSTRAINT [FK_RetryQueueStatus_RetryQueues] FOREIGN KEY ([IdStatus]) REFERENCES [dbo].[QueueStatus]([Code]),
		)
	PRINT 'Created table [RetryQueues]'
END
GO

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'RetryQueueItems'))
BEGIN
	CREATE TABLE [dbo].[RetryQueueItems] (
		[Id] [bigint] PRIMARY KEY IDENTITY(1,1),
		[IdDomain] [uniqueidentifier] NOT NULL,
		[IdRetryQueue] [bigint] NOT NULL, 
		[IdDomainRetryQueue] [uniqueidentifier] NOT NULL,
		[IdItemStatus] [tinyint] NOT NULL,
		[IdSeverityLevel] [tinyint] NOT NULL,
		[AttemptsCount] [int] NOT NULL,
		[Sort] [int] NOT NULL,
		[CreationDate] [datetime2](7) NOT NULL,
		[LastExecution] [datetime2](7),
		[ModifiedStatusDate] [datetime2](7),
		[Description] [nvarchar](MAX) NULL,
		CONSTRAINT [FK_RetryQueues_RetryQueueItems_IdRetryQueue] FOREIGN KEY ([IdRetryQueue]) REFERENCES [dbo].[RetryQueues]([Id]) ON DELETE CASCADE,
		CONSTRAINT [FK_RetryQueues_RetryQueueItems_IdDomainRetryQueue] FOREIGN KEY ([IdDomainRetryQueue]) REFERENCES [dbo].[RetryQueues]([IdDomain]),
		CONSTRAINT [FK_QueueItemStatus_RetryQueueItems] FOREIGN KEY ([IdItemStatus]) REFERENCES [dbo].[QueueItemStatus]([Code]),
		CONSTRAINT [FK_QueueItemSeverity_RetryQueueItems] FOREIGN KEY ([IdSeverityLevel]) REFERENCES [dbo].[QueueItemSeverity]([Code]),
		)
	PRINT 'Created table [RetryQueueItems]'
END
GO

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'ItemMessages'))
BEGIN
	CREATE TABLE [dbo].[ItemMessages] (
		[IdRetryQueueItem] [bigint] PRIMARY KEY,		
		[Key] [varbinary](8000) NOT NULL,
		[Value] [varbinary](MAX) NOT NULL,
		[TopicName] [nvarchar](300) NOT NULL,
		[Partition] [int] NOT NULL,
		[Offset] [bigint] NOT NULL,
		[UtcTimeStamp] [datetime2](7) NOT NULL,
		CONSTRAINT [FK_RetryQueueItems_ItemMessages] FOREIGN KEY ([IdRetryQueueItem]) REFERENCES [dbo].[RetryQueueItems]([Id]) ON DELETE CASCADE,
		)
	PRINT 'Created table [ItemMessages]'
END
GO

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'RetryItemMessageHeaders'))
BEGIN
	CREATE TABLE [dbo].[RetryItemMessageHeaders] (
		[Id] [bigint] PRIMARY KEY IDENTITY(1,1) NOT NULL,
		[IdItemMessage] [bigint] NOT NULL,
		[Key] [nvarchar](255) NOT NULL,
		[Value] [varbinary](8000) NOT NULL,
		CONSTRAINT [FK_ItemMessages_RetryItemMessageHeaders] FOREIGN KEY ([IdItemMessage]) REFERENCES [dbo].[ItemMessages]([IdRetryQueueItem]) ON DELETE CASCADE,
		)
	PRINT 'Created table [RetryItemMessageHeaders]'
END
GO

IF (NOT EXISTS (SELECT * 
				FROM sys.types 
				WHERE is_table_type = 1 
				AND name = 'TY_RetryQueueItemsIds'))
BEGIN
	CREATE TYPE TY_RetryQueueItemsIds AS TABLE (Id BIGINT)
END
GO

IF (EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('[dbo].[P_LoadItemMessages]')))
BEGIN
	DROP PROCEDURE [dbo].[P_LoadItemMessages]
END
GO

CREATE PROCEDURE [dbo].[P_LoadItemMessages]
	@RetryQueueItemsIds [dbo].[TY_RetryQueueItemsIds] READONLY
AS
	SET NOCOUNT ON;

	SELECT *
	FROM [ItemMessages] IM 
	INNER JOIN @RetryQueueItemsIds RI ON IM.[IdRetryQueueItem] = RI.[Id]
	INNER JOIN [RetryQueueItems] RQI ON RQI.[Id] = IM.[IdRetryQueueItem]
	ORDER BY RQI.IdRetryQueue, IM.IdRetryQueueItem
GO

-- CREATE INDEXES

-- Table [RetryQueues]

IF (NOT EXISTS (SELECT * 
FROM sys.indexes 
WHERE name='IX_RetryQueues_SearchGroupKey' AND object_id = OBJECT_ID('dbo.RetryQueues')))
BEGIN
	CREATE NONCLUSTERED INDEX IX_RetryQueues_SearchGroupKey ON [RetryQueues] ([SearchGroupKey]) 
END

IF (NOT EXISTS (SELECT * 
FROM sys.indexes 
WHERE name='IX_RetryQueues_QueueGroupKey' AND object_id = OBJECT_ID('dbo.RetryQueues')))
BEGIN
	CREATE UNIQUE NONCLUSTERED INDEX IX_RetryQueues_QueueGroupKey ON [RetryQueues] ([QueueGroupKey])
END

IF (NOT EXISTS (SELECT * 
FROM sys.indexes 
WHERE name='IX_RetryQueues_IdStatus' AND object_id = OBJECT_ID('dbo.RetryQueues')))
BEGIN
	CREATE NONCLUSTERED INDEX IX_RetryQueues_IdStatus ON [RetryQueues] ([IdStatus])
END

IF (NOT EXISTS (SELECT * 
FROM sys.indexes 
WHERE name='IX_RetryQueues_CreationDate' AND object_id = OBJECT_ID('dbo.RetryQueues')))
BEGIN
	CREATE NONCLUSTERED INDEX IX_RetryQueues_CreationDate ON [RetryQueues] ([CreationDate])
END

IF (NOT EXISTS (SELECT * 
FROM sys.indexes 
WHERE name='IX_RetryQueues_LastExecution' AND object_id = OBJECT_ID('dbo.RetryQueues')))
BEGIN
	CREATE NONCLUSTERED INDEX IX_RetryQueues_LastExecution ON [RetryQueues] ([LastExecution])
END

GO

-- Table [RetryQueuesItems]
IF (NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name='IX_RetryQueueItems_IdDomain' AND object_id = OBJECT_ID('dbo.RetryQueueItems')))
BEGIN
	CREATE UNIQUE NONCLUSTERED INDEX IX_RetryQueueItems_IdDomain ON [RetryQueueItems] ([IdDomain])	
END

IF (NOT EXISTS (SELECT * 
FROM sys.indexes 
WHERE name='IX_RetryQueueItems_Sort' AND object_id = OBJECT_ID('dbo.RetryQueueItems')))
BEGIN
	CREATE NONCLUSTERED INDEX IX_RetryQueueItems_Sort ON [RetryQueueItems] ([Sort])	
END

IF (NOT EXISTS (SELECT * 
FROM sys.indexes 
WHERE name='IX_RetryQueueItems_IdItemStatus' AND object_id = OBJECT_ID('dbo.RetryQueueItems')))
BEGIN
	CREATE NONCLUSTERED INDEX IX_RetryQueueItems_IdItemStatus ON [RetryQueueItems] ([IdItemStatus])	
END

IF (NOT EXISTS (SELECT * 
FROM sys.indexes 
WHERE name='IX_RetryQueueItems_IdSeverityLevel' AND object_id = OBJECT_ID('dbo.RetryQueueItems')))
BEGIN
	CREATE NONCLUSTERED INDEX IX_RetryQueueItems_IdSeverityLevel ON [RetryQueueItems] ([IdSeverityLevel])	
END