-- Create database if not exists.

USE master
IF (NOT EXISTS (SELECT * 
                 FROM sys.databases 
                 WHERE name = '@dbname'))
BEGIN
    CREATE DATABASE @dbname;
	PRINT 'Database @dbname Created';
END