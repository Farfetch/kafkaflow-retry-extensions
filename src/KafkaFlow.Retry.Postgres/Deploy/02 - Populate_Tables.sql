-- Populate tables

USE @dbname

IF (NOT EXISTS (SELECT * 
                 FROM [dbo].[QueueStatus] 
                 WHERE [Code] IN (1, 2)))
BEGIN

INSERT INTO [dbo].[QueueStatus] 
	VALUES
	(1, 'Active', 'The queue has unprocessed messages'),
	(2, 'Done', 'The queue does not have unprocessed messages')

END 

IF (NOT EXISTS (SELECT * 
                 FROM [dbo].[QueueItemStatus] 
                 WHERE [Code] IN (1, 2, 3)))
BEGIN

INSERT INTO [dbo].[QueueItemStatus] 
	VALUES
	(1, 'Waiting', 'Waiting for retry'),
	(2, 'InRetry', 'Retrying'),
	(3, 'Done', 'Done'),
	(4, 'Cancelled', 'Cancelled')

END

IF (NOT EXISTS (SELECT * 
                 FROM [dbo].[QueueItemSeverity] 
                 WHERE [Code] IN (1, 2, 3)))
BEGIN

INSERT INTO [dbo].[QueueItemSeverity] 
	VALUES
	(0, 'Unknown', 'A severity level was not defined.'),
	(1, 'Low', 'No loss of service. The software should recover by itself.'),
	(2, 'Medium', 'Minor loss of service. The result is an inconvenience, it''s unclear if the software can recover by itself.'),
	(3, 'High', 'Partial loss of service with severe impact on the business. Usually needs human intervention to be solved.')

END