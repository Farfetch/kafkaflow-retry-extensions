-- Populate tables

DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.QueueStatus WHERE Code IN (1, 2)) THEN
        INSERT INTO dbo.QueueStatus (Code, Name, Description)
        VALUES
            (1, 'Active', 'The queue has unprocessed messages'),
            (2, 'Done', 'The queue does not have unprocessed messages');
    END IF;

    IF NOT EXISTS (SELECT 1 FROM dbo.QueueItemStatus WHERE Code IN (1, 2, 3)) THEN
        INSERT INTO dbo.QueueItemStatus (Code, Name, Description)
        VALUES
            (1, 'Waiting', 'Waiting for retry'),
            (2, 'InRetry', 'Retrying'),
            (3, 'Done', 'Done'),
            (4, 'Cancelled', 'Cancelled');
    END IF;

    IF NOT EXISTS (SELECT 1 FROM dbo.QueueItemSeverity WHERE Code IN (1, 2, 3)) THEN
        INSERT INTO dbo.QueueItemSeverity (Code, Name, Description)
        VALUES
            (0, 'Unknown', 'A severity level was not defined.'),
            (1, 'Low', 'No loss of service. The software should recover by itself.'),
            (2, 'Medium', 'Minor loss of service. The result is an inconvenience, it''s unclear if the software can recover by itself.'),
            (3, 'High', 'Partial loss of service with severe impact on the business. Usually needs human intervention to be solved.');
    END IF;
END $$;